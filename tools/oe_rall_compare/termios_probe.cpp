#include <algorithm>
#include <cerrno>
#include <chrono>
#include <climits>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <fcntl.h>
#include <fstream>
#include <iostream>
#include <map>
#include <sstream>
#include <string>
#include <sys/select.h>
#include <sys/stat.h>
#include <sys/time.h>
#include <termios.h>
#include <thread>
#include <unistd.h>
#include <vector>

#ifdef __APPLE__
#include <IOKit/serial/ioss.h>
#include <sys/ioctl.h>
#endif

namespace {

constexpr size_t kFrameBytes = 12288;
constexpr size_t kCounterOffset = 12287;

struct Options {
    std::string port;
    std::string out_dir;
    int frames = 1200;
    int post_write_delay_ms = 30;
    int read_timeout_ms = 10000;
    int max_read_errors = 1;
};

struct CounterAudit {
    bool has_previous = false;
    uint64_t previous_ns = 0;
    uint8_t previous_counter = 0;
    int frames_audited = 0;
    int first_counter = -1;
    int last_counter = -1;
    int delta_1_count = 0;
    int delta_0_count = 0;
    int delta_gt1_count = 0;
    int estimated_missing_windows = 0;
    std::map<int, int> delta_counts;
    std::vector<double> gaps_ms;

    void record(uint64_t monotonic_ns, uint8_t counter) {
        if (first_counter < 0) {
            first_counter = counter;
        }
        if (has_previous) {
            int delta = (static_cast<int>(counter) - static_cast<int>(previous_counter)) & 0xFF;
            delta_counts[delta] += 1;
            gaps_ms.push_back(static_cast<double>(monotonic_ns - previous_ns) / 1'000'000.0);
            if (delta == 0) {
                delta_0_count += 1;
            } else if (delta == 1) {
                delta_1_count += 1;
            } else {
                delta_gt1_count += 1;
                estimated_missing_windows += delta - 1;
            }
        }
        has_previous = true;
        previous_ns = monotonic_ns;
        previous_counter = counter;
        last_counter = counter;
        frames_audited += 1;
    }
};

void usage(const char *argv0) {
    std::cerr << "Usage: " << argv0
              << " --port <path> --out-dir <path> [--frames <n>]"
              << " [--post-write-delay-ms <ms>] [--read-timeout-ms <ms>]"
              << " [--max-read-errors <n>]\n";
}

int parse_positive(const char *value, const char *name) {
    char *end = nullptr;
    long parsed = std::strtol(value, &end, 10);
    if (end == value || *end != '\0' || parsed <= 0 || parsed > INT_MAX) {
        std::cerr << name << " must be a positive integer: " << value << "\n";
        std::exit(2);
    }
    return static_cast<int>(parsed);
}

Options parse_args(int argc, char **argv) {
    Options options;
    for (int i = 1; i < argc; ++i) {
        std::string arg = argv[i];
        if (arg == "--port" && i + 1 < argc) {
            options.port = argv[++i];
        } else if (arg == "--out-dir" && i + 1 < argc) {
            options.out_dir = argv[++i];
        } else if (arg == "--frames" && i + 1 < argc) {
            options.frames = parse_positive(argv[++i], "--frames");
        } else if (arg == "--post-write-delay-ms" && i + 1 < argc) {
            options.post_write_delay_ms = parse_positive(argv[++i], "--post-write-delay-ms");
        } else if (arg == "--read-timeout-ms" && i + 1 < argc) {
            options.read_timeout_ms = parse_positive(argv[++i], "--read-timeout-ms");
        } else if (arg == "--max-read-errors" && i + 1 < argc) {
            options.max_read_errors = parse_positive(argv[++i], "--max-read-errors");
        } else {
            usage(argv[0]);
            std::exit(2);
        }
    }
    if (options.port.empty() || options.out_dir.empty()) {
        usage(argv[0]);
        std::exit(2);
    }
    return options;
}

uint64_t epoch_ms() {
    using namespace std::chrono;
    return duration_cast<milliseconds>(system_clock::now().time_since_epoch()).count();
}

std::string now_ts() {
    uint64_t ms = epoch_ms();
    char millis[8];
    std::snprintf(millis, sizeof(millis), "%03llu", static_cast<unsigned long long>(ms % 1000));
    return std::to_string(ms / 1000) + "." + millis + "Z";
}

uint64_t mono_ns() {
    using namespace std::chrono;
    return duration_cast<nanoseconds>(steady_clock::now().time_since_epoch()).count();
}

int mkdir_p(const std::string &path) {
    std::string current;
    for (char ch : path) {
        current.push_back(ch);
        if (ch == '/' && current.size() > 1) {
            if (::mkdir(current.c_str(), 0775) != 0 && errno != EEXIST) {
                return -1;
            }
        }
    }
    if (::mkdir(path.c_str(), 0775) != 0 && errno != EEXIST) {
        return -1;
    }
    return 0;
}

int configure_port(int fd) {
    termios tio{};
    if (tcgetattr(fd, &tio) != 0) {
        return -1;
    }
    cfmakeraw(&tio);
    tio.c_cflag |= CLOCAL | CREAD;
    tio.c_cflag &= ~PARENB;
    tio.c_cflag &= ~CSTOPB;
    tio.c_cflag &= ~CSIZE;
    tio.c_cflag |= CS8;
#ifdef CRTSCTS
    tio.c_cflag &= ~CRTSCTS;
#endif
#ifdef CCTS_OFLOW
    tio.c_cflag &= ~CCTS_OFLOW;
#endif
#ifdef CRTS_IFLOW
    tio.c_cflag &= ~CRTS_IFLOW;
#endif
    tio.c_iflag &= ~(IXON | IXOFF | IXANY);
    tio.c_cc[VMIN] = 0;
    tio.c_cc[VTIME] = 0;
#ifdef B921600
    cfsetispeed(&tio, B921600);
    cfsetospeed(&tio, B921600);
#else
    cfsetispeed(&tio, B9600);
    cfsetospeed(&tio, B9600);
#endif
    if (tcsetattr(fd, TCSANOW, &tio) != 0) {
        return -1;
    }
#if defined(__APPLE__) && !defined(B921600)
    speed_t speed = 921600;
    if (ioctl(fd, IOSSIOSPEED, &speed) == -1) {
        return -1;
    }
#endif
    tcflush(fd, TCIOFLUSH);
    return 0;
}

int write_all(int fd, const uint8_t *data, size_t len) {
    size_t offset = 0;
    while (offset < len) {
        ssize_t written = write(fd, data + offset, len - offset);
        if (written > 0) {
            offset += static_cast<size_t>(written);
        } else if (written < 0 && errno == EINTR) {
            continue;
        } else if (written < 0 && (errno == EAGAIN || errno == EWOULDBLOCK)) {
            std::this_thread::sleep_for(std::chrono::milliseconds(1));
        } else {
            return -1;
        }
    }
    return tcdrain(fd);
}

int read_exact_timeout(int fd, uint8_t *data, size_t len, int timeout_ms, size_t *actual) {
    uint64_t deadline = mono_ns() + static_cast<uint64_t>(timeout_ms) * 1'000'000ULL;
    *actual = 0;
    while (*actual < len) {
        uint64_t now = mono_ns();
        if (now >= deadline) {
            return 1;
        }
        uint64_t remaining = deadline - now;
        timeval tv{};
        tv.tv_sec = static_cast<time_t>(remaining / 1'000'000'000ULL);
        tv.tv_usec = static_cast<suseconds_t>((remaining % 1'000'000'000ULL) / 1000ULL);
        fd_set readfds;
        FD_ZERO(&readfds);
        FD_SET(fd, &readfds);
        int ready = select(fd + 1, &readfds, nullptr, nullptr, &tv);
        if (ready == 0) {
            return 1;
        }
        if (ready < 0) {
            if (errno == EINTR) {
                continue;
            }
            return -1;
        }
        ssize_t got = read(fd, data + *actual, len - *actual);
        if (got > 0) {
            *actual += static_cast<size_t>(got);
        } else if (got == 0) {
            std::this_thread::sleep_for(std::chrono::milliseconds(1));
        } else if (errno == EINTR || errno == EAGAIN || errno == EWOULDBLOCK) {
            continue;
        } else {
            return -1;
        }
    }
    return 0;
}

double percentile(std::vector<double> values, double p) {
    if (values.empty()) {
        return 0.0;
    }
    std::sort(values.begin(), values.end());
    size_t index = static_cast<size_t>((values.size() - 1) * p + 0.5);
    return values[index];
}

void write_delta_counts(std::ofstream &out, const CounterAudit &audit) {
    out << "[";
    bool first = true;
    for (const auto &[delta, count] : audit.delta_counts) {
        if (!first) {
            out << ",";
        }
        out << "{\"delta\":" << delta << ",\"count\":" << count << "}";
        first = false;
    }
    out << "]";
}

}  // namespace

int main(int argc, char **argv) {
    Options options = parse_args(argc, argv);
    std::string raw_dir = options.out_dir + "/raw";
    if (mkdir_p(raw_dir) != 0) {
        std::cerr << "mkdir failed: " << std::strerror(errno) << "\n";
        return 1;
    }

    std::ofstream raw_file(raw_dir + "/oe1022d.rall", std::ios::binary | std::ios::trunc);
    std::ofstream index_file(raw_dir + "/oe1022d.frames.idx.jsonl", std::ios::trunc);
    std::ofstream events_file(options.out_dir + "/events.jsonl", std::ios::trunc);
    if (!raw_file || !index_file || !events_file) {
        std::cerr << "open artifact failed\n";
        return 1;
    }

    int fd = open(options.port.c_str(), O_RDWR | O_NOCTTY | O_NONBLOCK);
    if (fd < 0) {
        std::cerr << "open port failed: " << std::strerror(errno) << "\n";
        return 1;
    }
    if (configure_port(fd) != 0) {
        std::cerr << "configure port failed: " << std::strerror(errno) << "\n";
        return 1;
    }

    std::string started_at = now_ts();
    uint64_t start_ns = mono_ns();
    CounterAudit audit;
    std::vector<uint8_t> payload(kFrameBytes);
    const uint8_t command[] = {'R', 'A', 'L', 'L', '?', '\r'};
    int frames_ok = 0;
    int read_attempts = 0;
    int read_errors = 0;
    int timeout_count = 0;
    int raw_len_bad_count = 0;
    uint64_t raw_offset = 0;

    while (frames_ok < options.frames) {
        read_attempts += 1;
        uint64_t read_start = mono_ns();
        if (write_all(fd, command, sizeof(command)) != 0) {
            std::cerr << "write failed: " << std::strerror(errno) << "\n";
            return 1;
        }
        std::this_thread::sleep_for(std::chrono::milliseconds(options.post_write_delay_ms));
        size_t actual = 0;
        int status = read_exact_timeout(fd, payload.data(), payload.size(), options.read_timeout_ms, &actual);
        double read_ms = static_cast<double>(mono_ns() - read_start) / 1'000'000.0;
        if (status != 0 || actual != kFrameBytes) {
            read_errors += 1;
            if (actual == 0) {
                timeout_count += 1;
            } else {
                raw_len_bad_count += 1;
            }
            events_file << "{\"ts\":\"" << now_ts() << "\",\"monotonic_ns\":"
                        << (mono_ns() - start_ns)
                        << ",\"event\":\"rall_read_error\",\"data\":{\"read_attempt\":"
                        << read_attempts << ",\"frames_ok\":" << frames_ok
                        << ",\"raw_len\":" << actual << ",\"elapsed_ms\":" << read_ms
                        << ",\"errno\":" << (status < 0 ? errno : 0) << "}}\n";
            events_file.flush();
            if (read_errors >= options.max_read_errors) {
                break;
            }
            continue;
        }

        uint64_t frame_ns = mono_ns() - start_ns;
        raw_file.write(reinterpret_cast<const char *>(payload.data()), payload.size());
        raw_file.flush();
        index_file << "{\"frame_seq\":" << frames_ok << ",\"ts\":\"" << now_ts()
                   << "\",\"monotonic_ns\":" << frame_ns << ",\"raw_offset\":" << raw_offset
                   << ",\"raw_len\":" << kFrameBytes
                   << ",\"parse_status\":\"not_parsed\",\"duplicate_of\":null}\n";
        index_file.flush();
        audit.record(frame_ns, payload[kCounterOffset]);
        raw_offset += kFrameBytes;
        frames_ok += 1;
    }

    close(fd);

    std::ofstream summary(options.out_dir + "/summary.json", std::ios::trunc);
    std::vector<double> gaps = audit.gaps_ms;
    double median = percentile(gaps, 0.5);
    double p95 = percentile(gaps, 0.95);
    double p99 = percentile(gaps, 0.99);
    double max_gap = gaps.empty() ? 0.0 : *std::max_element(gaps.begin(), gaps.end());
    summary << "{\n"
            << "  \"tool\": \"termios_probe_cpp\",\n"
            << "  \"port_path\": \"" << options.port << "\",\n"
            << "  \"baud_rate\": 921600,\n"
            << "  \"command\": \"RALL?\",\n"
            << "  \"frame_bytes\": " << kFrameBytes << ",\n"
            << "  \"post_write_delay_ms\": " << options.post_write_delay_ms << ",\n"
            << "  \"read_timeout_ms\": " << options.read_timeout_ms << ",\n"
            << "  \"max_read_errors\": " << options.max_read_errors << ",\n"
            << "  \"started_at\": \"" << started_at << "\",\n"
            << "  \"ended_at\": \"" << now_ts() << "\",\n"
            << "  \"elapsed_ms\": " << static_cast<double>(mono_ns() - start_ns) / 1'000'000.0
            << ",\n"
            << "  \"frames_requested\": " << options.frames << ",\n"
            << "  \"frames_ok\": " << frames_ok << ",\n"
            << "  \"read_attempts\": " << read_attempts << ",\n"
            << "  \"read_errors\": " << read_errors << ",\n"
            << "  \"timeout_count\": " << timeout_count << ",\n"
            << "  \"writer\": {\n"
            << "    \"frames_written\": " << frames_ok << ",\n"
            << "    \"raw_len_bad_count\": " << raw_len_bad_count << ",\n"
            << "    \"packet_counter\": {\n"
            << "      \"offset\": " << kCounterOffset << ",\n"
            << "      \"frames_audited\": " << audit.frames_audited << ",\n"
            << "      \"boundaries_evaluated\": " << std::max(0, audit.frames_audited - 1) << ",\n"
            << "      \"first_counter\": " << audit.first_counter << ",\n"
            << "      \"last_counter\": " << audit.last_counter << ",\n"
            << "      \"delta_1_count\": " << audit.delta_1_count << ",\n"
            << "      \"delta_0_count\": " << audit.delta_0_count << ",\n"
            << "      \"delta_gt1_count\": " << audit.delta_gt1_count << ",\n"
            << "      \"estimated_missing_windows\": " << audit.estimated_missing_windows << ",\n"
            << "      \"delta_counts\": ";
    write_delta_counts(summary, audit);
    summary << ",\n"
            << "      \"gap_median_ms\": " << median << ",\n"
            << "      \"gap_p95_ms\": " << p95 << ",\n"
            << "      \"gap_p99_ms\": " << p99 << ",\n"
            << "      \"gap_max_ms\": " << max_gap << "\n"
            << "    }\n"
            << "  },\n"
            << "  \"raw_file\": \"raw/oe1022d.rall\",\n"
            << "  \"index_file\": \"raw/oe1022d.frames.idx.jsonl\"\n"
            << "}\n";

    std::cout << "C++ termios OE RALL probe complete: frames_ok=" << frames_ok
              << ", timeout_count=" << timeout_count
              << ", counter_delta_gt1=" << audit.delta_gt1_count
              << ", out_dir=" << options.out_dir << "\n";
    return 0;
}
