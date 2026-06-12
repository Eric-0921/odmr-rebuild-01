#include <errno.h>
#include <fcntl.h>
#include <limits.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/select.h>
#include <sys/stat.h>
#include <sys/time.h>
#include <termios.h>
#include <time.h>
#include <unistd.h>

#ifdef __APPLE__
#include <IOKit/serial/ioss.h>
#include <sys/ioctl.h>
#endif

#define FRAME_BYTES 12288
#define COUNTER_OFFSET 12287

typedef struct {
    const char *port;
    const char *out_dir;
    int frames;
    int post_write_delay_ms;
    int read_timeout_ms;
    int max_read_errors;
} Options;

typedef struct {
    int has_previous;
    uint64_t previous_ns;
    unsigned int previous_counter;
    int frames_audited;
    int first_counter;
    int last_counter;
    int delta_1_count;
    int delta_0_count;
    int delta_gt1_count;
    int estimated_missing_windows;
    int delta_counts[256];
    double gap_median_ms;
    double gap_p95_ms;
    double gap_p99_ms;
    double gap_max_ms;
    double *gaps_ms;
    int gaps_len;
    int gaps_cap;
} CounterAudit;

static void usage(const char *argv0) {
    fprintf(stderr,
            "Usage: %s --port <path> --out-dir <path> [--frames <n>] "
            "[--post-write-delay-ms <ms>] [--read-timeout-ms <ms>] "
            "[--max-read-errors <n>]\n",
            argv0);
}

static int parse_int(const char *value, const char *name) {
    char *end = NULL;
    long parsed = strtol(value, &end, 10);
    if (end == value || *end != '\0' || parsed <= 0 || parsed > INT_MAX) {
        fprintf(stderr, "%s must be a positive integer: %s\n", name, value);
        exit(2);
    }
    return (int)parsed;
}

static Options parse_args(int argc, char **argv) {
    Options options;
    options.port = NULL;
    options.out_dir = NULL;
    options.frames = 1200;
    options.post_write_delay_ms = 30;
    options.read_timeout_ms = 10000;
    options.max_read_errors = 1;

    for (int i = 1; i < argc; i++) {
        if (strcmp(argv[i], "--port") == 0 && i + 1 < argc) {
            options.port = argv[++i];
        } else if (strcmp(argv[i], "--out-dir") == 0 && i + 1 < argc) {
            options.out_dir = argv[++i];
        } else if (strcmp(argv[i], "--frames") == 0 && i + 1 < argc) {
            options.frames = parse_int(argv[++i], "--frames");
        } else if (strcmp(argv[i], "--post-write-delay-ms") == 0 && i + 1 < argc) {
            options.post_write_delay_ms = parse_int(argv[++i], "--post-write-delay-ms");
        } else if (strcmp(argv[i], "--read-timeout-ms") == 0 && i + 1 < argc) {
            options.read_timeout_ms = parse_int(argv[++i], "--read-timeout-ms");
        } else if (strcmp(argv[i], "--max-read-errors") == 0 && i + 1 < argc) {
            options.max_read_errors = parse_int(argv[++i], "--max-read-errors");
        } else {
            usage(argv[0]);
            exit(2);
        }
    }

    if (options.port == NULL || options.out_dir == NULL) {
        usage(argv[0]);
        exit(2);
    }
    return options;
}

static uint64_t monotonic_now_ns(void) {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return (uint64_t)ts.tv_sec * 1000000000ULL + (uint64_t)ts.tv_nsec;
}

static void now_ts(char *out, size_t out_len) {
    struct timeval tv;
    gettimeofday(&tv, NULL);
    snprintf(out, out_len, "%lld.%03dZ", (long long)tv.tv_sec, (int)(tv.tv_usec / 1000));
}

static void sleep_ms(int ms) {
    struct timespec req;
    req.tv_sec = ms / 1000;
    req.tv_nsec = (long)(ms % 1000) * 1000000L;
    while (nanosleep(&req, &req) != 0 && errno == EINTR) {
    }
}

static int mkdir_p(const char *path) {
    char tmp[PATH_MAX];
    size_t len = strlen(path);
    if (len >= sizeof(tmp)) {
        errno = ENAMETOOLONG;
        return -1;
    }
    strcpy(tmp, path);
    for (char *p = tmp + 1; *p; p++) {
        if (*p == '/') {
            *p = '\0';
            if (mkdir(tmp, 0775) != 0 && errno != EEXIST) {
                return -1;
            }
            *p = '/';
        }
    }
    if (mkdir(tmp, 0775) != 0 && errno != EEXIST) {
        return -1;
    }
    return 0;
}

static int configure_port(int fd, int baud_rate) {
    struct termios tio;
    if (tcgetattr(fd, &tio) != 0) {
        return -1;
    }
    cfmakeraw(&tio);
    tio.c_cflag |= (CLOCAL | CREAD);
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
    speed_t speed = (speed_t)baud_rate;
    if (ioctl(fd, IOSSIOSPEED, &speed) == -1) {
        return -1;
    }
#else
    (void)baud_rate;
#endif

    tcflush(fd, TCIOFLUSH);
    return 0;
}

static int write_all(int fd, const uint8_t *buf, size_t len) {
    size_t offset = 0;
    while (offset < len) {
        ssize_t written = write(fd, buf + offset, len - offset);
        if (written > 0) {
            offset += (size_t)written;
        } else if (written < 0 && errno == EINTR) {
            continue;
        } else if (written < 0 && (errno == EAGAIN || errno == EWOULDBLOCK)) {
            sleep_ms(1);
        } else {
            return -1;
        }
    }
    return tcdrain(fd);
}

static int read_exact_timeout(int fd, uint8_t *buf, size_t len, int timeout_ms, size_t *actual) {
    uint64_t deadline = monotonic_now_ns() + (uint64_t)timeout_ms * 1000000ULL;
    *actual = 0;
    while (*actual < len) {
        uint64_t now = monotonic_now_ns();
        if (now >= deadline) {
            return 1;
        }
        uint64_t remaining_ns = deadline - now;
        struct timeval tv;
        tv.tv_sec = (time_t)(remaining_ns / 1000000000ULL);
        tv.tv_usec = (suseconds_t)((remaining_ns % 1000000000ULL) / 1000ULL);
        fd_set readfds;
        FD_ZERO(&readfds);
        FD_SET(fd, &readfds);
        int ready = select(fd + 1, &readfds, NULL, NULL, &tv);
        if (ready == 0) {
            return 1;
        }
        if (ready < 0) {
            if (errno == EINTR) {
                continue;
            }
            return -1;
        }
        ssize_t got = read(fd, buf + *actual, len - *actual);
        if (got > 0) {
            *actual += (size_t)got;
        } else if (got == 0) {
            sleep_ms(1);
        } else if (errno == EINTR || errno == EAGAIN || errno == EWOULDBLOCK) {
            continue;
        } else {
            return -1;
        }
    }
    return 0;
}

static int compare_double(const void *a, const void *b) {
    double da = *(const double *)a;
    double db = *(const double *)b;
    return (da > db) - (da < db);
}

static void counter_init(CounterAudit *audit) {
    memset(audit, 0, sizeof(*audit));
    audit->first_counter = -1;
    audit->last_counter = -1;
}

static void counter_record(CounterAudit *audit, uint64_t monotonic_ns, unsigned int counter) {
    if (audit->first_counter < 0) {
        audit->first_counter = (int)counter;
    }
    if (audit->has_previous) {
        unsigned int delta = (counter + 256U - audit->previous_counter) & 0xFFU;
        audit->delta_counts[delta]++;
        if (audit->gaps_len == audit->gaps_cap) {
            audit->gaps_cap = audit->gaps_cap == 0 ? 1024 : audit->gaps_cap * 2;
            audit->gaps_ms = (double *)realloc(audit->gaps_ms, sizeof(double) * audit->gaps_cap);
            if (audit->gaps_ms == NULL) {
                fprintf(stderr, "out of memory\n");
                exit(1);
            }
        }
        audit->gaps_ms[audit->gaps_len++] =
            (double)(monotonic_ns - audit->previous_ns) / 1000000.0;
        if (delta == 0) {
            audit->delta_0_count++;
        } else if (delta == 1) {
            audit->delta_1_count++;
        } else {
            audit->delta_gt1_count++;
            audit->estimated_missing_windows += (int)delta - 1;
        }
    }
    audit->has_previous = 1;
    audit->previous_ns = monotonic_ns;
    audit->previous_counter = counter;
    audit->last_counter = (int)counter;
    audit->frames_audited++;
}

static void counter_finish(CounterAudit *audit) {
    if (audit->gaps_len == 0) {
        return;
    }
    qsort(audit->gaps_ms, (size_t)audit->gaps_len, sizeof(double), compare_double);
    audit->gap_median_ms = audit->gaps_ms[audit->gaps_len / 2];
    audit->gap_p95_ms = audit->gaps_ms[(int)((audit->gaps_len - 1) * 0.95 + 0.5)];
    audit->gap_p99_ms = audit->gaps_ms[(int)((audit->gaps_len - 1) * 0.99 + 0.5)];
    audit->gap_max_ms = audit->gaps_ms[audit->gaps_len - 1];
}

static void write_delta_counts(FILE *file, const CounterAudit *audit) {
    int first = 1;
    fprintf(file, "[");
    for (int delta = 0; delta < 256; delta++) {
        if (audit->delta_counts[delta] == 0) {
            continue;
        }
        if (!first) {
            fprintf(file, ",");
        }
        fprintf(file, "{\"delta\":%d,\"count\":%d}", delta, audit->delta_counts[delta]);
        first = 0;
    }
    fprintf(file, "]");
}

int main(int argc, char **argv) {
    Options options = parse_args(argc, argv);
    char raw_dir[PATH_MAX], raw_path[PATH_MAX], index_path[PATH_MAX], events_path[PATH_MAX],
        summary_path[PATH_MAX];
    snprintf(raw_dir, sizeof(raw_dir), "%s/raw", options.out_dir);
    snprintf(raw_path, sizeof(raw_path), "%s/oe1022d.rall", raw_dir);
    snprintf(index_path, sizeof(index_path), "%s/oe1022d.frames.idx.jsonl", raw_dir);
    snprintf(events_path, sizeof(events_path), "%s/events.jsonl", options.out_dir);
    snprintf(summary_path, sizeof(summary_path), "%s/summary.json", options.out_dir);

    if (mkdir_p(raw_dir) != 0) {
        fprintf(stderr, "mkdir failed: %s\n", strerror(errno));
        return 1;
    }

    FILE *raw_file = fopen(raw_path, "wb");
    FILE *index_file = fopen(index_path, "w");
    FILE *events_file = fopen(events_path, "w");
    if (raw_file == NULL || index_file == NULL || events_file == NULL) {
        fprintf(stderr, "open artifact failed: %s\n", strerror(errno));
        return 1;
    }

    int fd = open(options.port, O_RDWR | O_NOCTTY | O_NONBLOCK);
    if (fd < 0) {
        fprintf(stderr, "open port failed: %s\n", strerror(errno));
        return 1;
    }
    if (configure_port(fd, 921600) != 0) {
        fprintf(stderr, "configure port failed: %s\n", strerror(errno));
        return 1;
    }

    char started_at[64], ended_at[64], ts[64];
    now_ts(started_at, sizeof(started_at));
    uint64_t start_ns = monotonic_now_ns();
    CounterAudit audit;
    counter_init(&audit);
    uint8_t payload[FRAME_BYTES];
    const uint8_t command[] = {'R', 'A', 'L', 'L', '?', '\r'};
    int frames_ok = 0, read_attempts = 0, read_errors = 0, timeout_count = 0,
        raw_len_bad_count = 0;
    uint64_t raw_offset = 0;

    while (frames_ok < options.frames) {
        read_attempts++;
        uint64_t read_started = monotonic_now_ns();
        if (write_all(fd, command, sizeof(command)) != 0) {
            fprintf(stderr, "write failed: %s\n", strerror(errno));
            return 1;
        }
        sleep_ms(options.post_write_delay_ms);
        size_t actual = 0;
        int read_status = read_exact_timeout(fd, payload, FRAME_BYTES, options.read_timeout_ms, &actual);
        double elapsed_ms = (double)(monotonic_now_ns() - read_started) / 1000000.0;
        if (read_status != 0 || actual != FRAME_BYTES) {
            read_errors++;
            if (actual == 0) {
                timeout_count++;
            } else {
                raw_len_bad_count++;
            }
            now_ts(ts, sizeof(ts));
            fprintf(events_file,
                    "{\"ts\":\"%s\",\"monotonic_ns\":%llu,\"event\":\"rall_read_error\","
                    "\"data\":{\"read_attempt\":%d,\"frames_ok\":%d,\"raw_len\":%zu,"
                    "\"elapsed_ms\":%.6f,\"errno\":%d}}\n",
                    ts, (unsigned long long)(monotonic_now_ns() - start_ns), read_attempts,
                    frames_ok, actual, elapsed_ms, read_status < 0 ? errno : 0);
            fflush(events_file);
            if (read_errors >= options.max_read_errors) {
                break;
            }
            continue;
        }

        uint64_t frame_ns = monotonic_now_ns() - start_ns;
        fwrite(payload, 1, FRAME_BYTES, raw_file);
        fflush(raw_file);
        now_ts(ts, sizeof(ts));
        fprintf(index_file,
                "{\"frame_seq\":%d,\"ts\":\"%s\",\"monotonic_ns\":%llu,"
                "\"raw_offset\":%llu,\"raw_len\":%d,\"parse_status\":\"not_parsed\","
                "\"duplicate_of\":null}\n",
                frames_ok, ts, (unsigned long long)frame_ns, (unsigned long long)raw_offset,
                FRAME_BYTES);
        fflush(index_file);
        counter_record(&audit, frame_ns, payload[COUNTER_OFFSET]);
        raw_offset += FRAME_BYTES;
        frames_ok++;
    }

    close(fd);
    fclose(raw_file);
    fclose(index_file);
    fclose(events_file);
    counter_finish(&audit);
    now_ts(ended_at, sizeof(ended_at));

    FILE *summary = fopen(summary_path, "w");
    if (summary == NULL) {
        fprintf(stderr, "open summary failed: %s\n", strerror(errno));
        return 1;
    }
    fprintf(summary,
            "{\n"
            "  \"tool\": \"termios_probe_c\",\n"
            "  \"port_path\": \"%s\",\n"
            "  \"baud_rate\": 921600,\n"
            "  \"command\": \"RALL?\",\n"
            "  \"frame_bytes\": %d,\n"
            "  \"post_write_delay_ms\": %d,\n"
            "  \"read_timeout_ms\": %d,\n"
            "  \"max_read_errors\": %d,\n"
            "  \"started_at\": \"%s\",\n"
            "  \"ended_at\": \"%s\",\n"
            "  \"elapsed_ms\": %.6f,\n"
            "  \"frames_requested\": %d,\n"
            "  \"frames_ok\": %d,\n"
            "  \"read_attempts\": %d,\n"
            "  \"read_errors\": %d,\n"
            "  \"timeout_count\": %d,\n"
            "  \"writer\": {\n"
            "    \"frames_written\": %d,\n"
            "    \"raw_len_bad_count\": %d,\n"
            "    \"packet_counter\": {\n"
            "      \"offset\": %d,\n"
            "      \"frames_audited\": %d,\n"
            "      \"boundaries_evaluated\": %d,\n"
            "      \"first_counter\": %d,\n"
            "      \"last_counter\": %d,\n"
            "      \"delta_1_count\": %d,\n"
            "      \"delta_0_count\": %d,\n"
            "      \"delta_gt1_count\": %d,\n"
            "      \"estimated_missing_windows\": %d,\n"
            "      \"delta_counts\": ",
            options.port, FRAME_BYTES, options.post_write_delay_ms, options.read_timeout_ms,
            options.max_read_errors, started_at, ended_at,
            (double)(monotonic_now_ns() - start_ns) / 1000000.0, options.frames, frames_ok,
            read_attempts, read_errors, timeout_count, frames_ok, raw_len_bad_count,
            COUNTER_OFFSET, audit.frames_audited, audit.frames_audited > 0 ? audit.frames_audited - 1 : 0,
            audit.first_counter, audit.last_counter, audit.delta_1_count, audit.delta_0_count,
            audit.delta_gt1_count, audit.estimated_missing_windows);
    write_delta_counts(summary, &audit);
    fprintf(summary,
            ",\n"
            "      \"gap_median_ms\": %.6f,\n"
            "      \"gap_p95_ms\": %.6f,\n"
            "      \"gap_p99_ms\": %.6f,\n"
            "      \"gap_max_ms\": %.6f\n"
            "    }\n"
            "  },\n"
            "  \"raw_file\": \"raw/oe1022d.rall\",\n"
            "  \"index_file\": \"raw/oe1022d.frames.idx.jsonl\"\n"
            "}\n",
            audit.gap_median_ms, audit.gap_p95_ms, audit.gap_p99_ms, audit.gap_max_ms);
    fclose(summary);
    free(audit.gaps_ms);

    printf("C termios OE RALL probe complete: frames_ok=%d, timeout_count=%d, "
           "counter_delta_gt1=%d, out_dir=%s\n",
           frames_ok, timeout_count, audit.delta_gt1_count, options.out_dir);
    return 0;
}
