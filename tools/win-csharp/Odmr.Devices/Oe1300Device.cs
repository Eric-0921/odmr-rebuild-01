using System.Globalization;
using System.IO.Ports;
using System.Net.Sockets;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json.Serialization;

namespace Odmr.Devices;

public static class Oe1300Defaults
{
    public const int SerialBaudRate = 115200;
    public const int SerialTimeoutMs = 1000;
    public const int SerialOpenSettleMs = 100;
    public const int SerialAsciiMaxBytes = 4096;
    public const string Host = "192.168.1.1";
    public const int TcpPort = 10001;
    public const int TcpConnectTimeoutMs = 3000;
    public const int TcpReadTimeoutMs = 300;
    public const int TcpAsciiMaxBytes = 4096;
    public const int TcpRallExpectedBytes = 32768;
    public const int TcpRallPayloadBytes = 29600;
    public const int TcpRallFrameBytes = 400;
    public const int TcpRallFrameCount = TcpRallPayloadBytes / TcpRallFrameBytes;
    public const int TcpRallStatusOffset = 29990;
    public const int TcpRallTrigCountOffset = 29997;
    public const int TcpRallAuxIn2Offset = 28432;
    public const int TcpRallAuxIn1Offset = 28656;
    public const int SerialRallFieldCount = 37;
    public const string RequiredIdnPrefix = "SSI LIA-OE130";

    public static readonly IReadOnlyList<string> SerialRallFieldNames =
    [
        "X", "Y", "R", "Theta",
        "XD1", "YD1", "RD1", "ThetaD1",
        "XD2", "YD2", "RD2", "ThetaD2",
        "XD3", "YD3", "RD3", "ThetaD3",
        "XD4", "YD4", "RD4", "ThetaD4",
        "XD5", "YD5", "RD5", "ThetaD5",
        "XD6", "YD6", "RD6", "ThetaD6",
        "XD7", "YD7", "RD7", "ThetaD7",
        "XNoise", "YNoise", "Frequency", "AuxIn1", "AuxIn2"
    ];
}

public static class Oe1300Commands
{
    public const string QueryIdn = "*IDN?";
    public const string QueryPll = "*PLL?";
    public const string QueryBaud = "BAUD?";
    public const string QueryInputSource = "ISRC?";
    public const string QueryInputCoupling = "ICPL?";
    public const string QueryInputRange = "IRNG?";
    public const string QueryReferenceSource = "FMOD?";
    public const string QueryReferenceFrequency = "FREQ?";
    public const string QueryReferenceSlope = "RSLP?";
    public const string QueryTimeConstant = "OFLT?";
    public const string QueryFilterSlope = "OFSL?";
    public const string QuerySync = "SYNC?";
    public const string QueryOverload = "OVLD?";
    public const string QueryTemperature = "TEMP?";
    public const string QueryRall = "RALL?";
    public const string QueryNetworkMode = "NMOD?";
    public const string QueryIpAddress = "NIPA?";
    public const string QuerySubnetMask = "NSMA?";
    public const string QueryGateway = "NGWA?";
    public const string QueryMac = "NMAC?";

    public static string QueryPid(int channel, int slot) => $"*PID? {channel},{slot}";

    public static string QueryPhase(int demodulator) => $"PHAS? {demodulator}";

    public static string QueryHarmonic(int demodulator) => $"HARM? {demodulator}";

    public static string QueryDemodulatorMode(int demodulator) => $"DMOD? {demodulator}";

    public static string QueryArbitraryFrequency(int demodulator) => $"DARB? {demodulator}";

    public static string QueryChannelOutputSource(int channel) => $"COUT? {channel}";

    public static string QueryChannelAuxOutput(int channel) => $"CAUX? {channel}";

    public static string QueryChannelOffset(int channel) => $"COFP? {channel}";

    public static string QueryChannelExpand(int channel) => $"CEXP? {channel}";

    public static string QueryAuxInput(int channel) => $"OAUX? {channel}";

    public static string QueryOutput(int parameterIndex) => $"OUTP? {parameterIndex}";

    public static string QuerySnap(params int[] parameterIndices) =>
        $"SNAP? {string.Join(",", parameterIndices)}";

    public static string SetInputSource(int source) => $"ISRC {source}";

    public static string SetInputCoupling(int coupling) => $"ICPL {coupling}";

    public static string SetInputRange(int range) => $"IRNG {range}";

    public static string SetReferenceSource(int source) => $"FMOD {source}";

    public static string SetReferenceFrequency(double hz) =>
        $"FREQ {hz.ToString(CultureInfo.InvariantCulture)}";

    public static string SetPhaseDeg(int demodulator, double degree) =>
        $"PHAS {demodulator},{degree.ToString(CultureInfo.InvariantCulture)}";

    public static string SetReferenceSlope(int slope) => $"RSLP {slope}";

    public static string SetHarmonic(int demodulator, int harmonic) => $"HARM {demodulator},{harmonic}";

    public static string SetDemodulatorMode(int demodulator, int mode) => $"DMOD {demodulator},{mode}";

    public static string SetArbitraryFrequency(int demodulator, double hz) =>
        $"DARB {demodulator},{hz.ToString(CultureInfo.InvariantCulture)}";

    public static string SetSensitivity(int index) => $"SENS {index}";

    public static string SetTimeConstant(double seconds) =>
        $"OFLT {seconds.ToString(CultureInfo.InvariantCulture)}";

    public static string SetFilterSlope(int slope) => $"OFSL {slope}";

    public static string SetSync(bool enabled) => $"SYNC {(enabled ? 1 : 0)}";

    public static string SetSineOutputEnabled(bool enabled) => $"SWVT {(enabled ? 1 : 0)}";

    public static string SetSineOutputVoltageVrms(double vrms) =>
        $"SLVL {vrms.ToString(CultureInfo.InvariantCulture)}";

    public static string SetChannelOutputSource(int channel, int source) => $"COUT {channel},{source}";

    public static string SetChannelAuxOutput(int channel, double volts) =>
        $"CAUX {channel},{volts.ToString(CultureInfo.InvariantCulture)}";

    public static string SetChannelOffset(int channel, double percent) =>
        $"COFP {channel},{percent.ToString(CultureInfo.InvariantCulture)}";

    public static string SetChannelExpand(int channel, int expand) => $"CEXP {channel},{expand}";
}

public sealed record Oe1300RallSnapshot(
    [property: JsonPropertyName("raw_response")] string RawResponse,
    [property: JsonPropertyName("values")] IReadOnlyList<double> Values,
    [property: JsonPropertyName("named_values")] IReadOnlyDictionary<string, double> NamedValues);

public sealed record Oe1300TcpRallFrameSummary(
    [property: JsonPropertyName("frame_index")] int FrameIndex,
    [property: JsonPropertyName("mean")] double Mean,
    [property: JsonPropertyName("min")] double Min,
    [property: JsonPropertyName("max")] double Max);

public sealed record Oe1300TcpRallCapture(
    [property: JsonPropertyName("total_bytes")] int TotalBytes,
    [property: JsonPropertyName("payload_bytes")] int PayloadBytes,
    [property: JsonPropertyName("frame_count")] int FrameCount,
    [property: JsonPropertyName("status_byte")] byte StatusByte,
    [property: JsonPropertyName("status_bit_0")] bool StatusBit0,
    [property: JsonPropertyName("status_bit_1")] bool StatusBit1,
    [property: JsonPropertyName("trig_count")] byte TrigCount,
    [property: JsonPropertyName("head_hex")] string HeadHex,
    [property: JsonPropertyName("tail_hex")] string TailHex,
    [property: JsonPropertyName("decode_mode")] string DecodeMode,
    [property: JsonPropertyName("named_values")] IReadOnlyDictionary<string, double> NamedValues,
    [property: JsonPropertyName("frame_summaries")] IReadOnlyList<Oe1300TcpRallFrameSummary> FrameSummaries);

public static class Oe1300Parsers
{
    public static Oe1300RallSnapshot ParseSerialRall(string response)
    {
        var values = ParseCsvDoubles(response, Oe1300Defaults.SerialRallFieldCount);
        return new Oe1300RallSnapshot(response, values, ToNamedValues(Oe1300Defaults.SerialRallFieldNames, values));
    }

    public static Oe1300TcpRallCapture DecodeTcpRall(byte[] payload)
    {
        if (payload.Length < Oe1300Defaults.TcpRallExpectedBytes)
        {
            throw new IOException($"OE1300 TCP RALL payload too short: expected >= {Oe1300Defaults.TcpRallExpectedBytes}, actual={payload.Length}");
        }

        var frameSummaries = new List<Oe1300TcpRallFrameSummary>(Oe1300Defaults.TcpRallFrameCount);

        for (var frameIndex = 0; frameIndex < Oe1300Defaults.TcpRallFrameCount; frameIndex++)
        {
            var frameValues = ReadFrameValues(payload, frameIndex);
            frameSummaries.Add(new Oe1300TcpRallFrameSummary(
                frameIndex,
                frameValues.Average(),
                frameValues.Min(),
                frameValues.Max()));
        }

        var statusByte = payload[Oe1300Defaults.TcpRallStatusOffset];
        var trigCount = payload[Oe1300Defaults.TcpRallTrigCountOffset];
        var namedValues = DecodeTcpNamedValues(payload, frameSummaries);

        return new Oe1300TcpRallCapture(
            payload.Length,
            Oe1300Defaults.TcpRallPayloadBytes,
            frameSummaries.Count,
            statusByte,
            (statusByte & 0x01) != 0,
            (statusByte & 0x02) != 0,
            trigCount,
            ToHexPrefix(payload, 32),
            ToHexSuffix(payload, 32),
            "frame_mean_big_endian_v1",
            namedValues,
            frameSummaries);
    }

    private static IReadOnlyList<double> ParseCsvDoubles(string response, int expectedCount)
    {
        var fields = response
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (fields.Length != expectedCount)
        {
            throw new IOException($"OE1300 ASCII response field count mismatch: expected {expectedCount}, actual={fields.Length}");
        }

        var values = new double[fields.Length];
        for (var i = 0; i < fields.Length; i++)
        {
            values[i] = double.Parse(fields[i], CultureInfo.InvariantCulture);
        }

        return values;
    }

    private static IReadOnlyDictionary<string, double> ToNamedValues(IReadOnlyList<string> names, IReadOnlyList<double> values)
    {
        var map = new Dictionary<string, double>(names.Count, StringComparer.Ordinal);
        for (var i = 0; i < names.Count; i++)
        {
            map[names[i]] = values[i];
        }

        return map;
    }

    private static IReadOnlyDictionary<string, double> DecodeTcpNamedValues(
        byte[] payload,
        IReadOnlyList<Oe1300TcpRallFrameSummary> frameSummaries)
    {
        var namedValues = new Dictionary<string, double>(Oe1300Defaults.SerialRallFieldCount, StringComparer.Ordinal)
        {
            ["X"] = MeanFramePair(frameSummaries, 0),
            ["Y"] = MeanFramePair(frameSummaries, 2),
            ["R"] = MeanFramePair(frameSummaries, 4),
            ["Theta"] = MeanFramePair(frameSummaries, 6),
            ["XNoise"] = MeanFramePair(frameSummaries, 8),
            ["YNoise"] = MeanFramePair(frameSummaries, 10),
            ["Frequency"] = MeanFramePair(frameSummaries, 12),
            ["AuxIn1"] = ReadBigEndianDouble(payload, Oe1300Defaults.TcpRallAuxIn1Offset),
            ["AuxIn2"] = ReadBigEndianDouble(payload, Oe1300Defaults.TcpRallAuxIn2Offset)
        };

        for (var harmonic = 1; harmonic <= 7; harmonic++)
        {
            var frameBase = 14 + ((harmonic - 1) * 8);
            namedValues[$"XD{harmonic}"] = MeanFramePair(frameSummaries, frameBase);
            namedValues[$"YD{harmonic}"] = MeanFramePair(frameSummaries, frameBase + 2);
            namedValues[$"RD{harmonic}"] = MeanFramePair(frameSummaries, frameBase + 4);
            namedValues[$"ThetaD{harmonic}"] = MeanFramePair(frameSummaries, frameBase + 6);
        }

        return namedValues;
    }

    private static IReadOnlyList<double> ReadFrameValues(byte[] payload, int frameIndex)
    {
        var frameOffset = frameIndex * Oe1300Defaults.TcpRallFrameBytes;
        var values = new double[Oe1300Defaults.TcpRallFrameBytes / sizeof(double)];

        for (var i = 0; i < values.Length; i++)
        {
            values[i] = ReadBigEndianDouble(payload, frameOffset + (i * sizeof(double)));
        }

        return values;
    }

    private static double MeanFramePair(IReadOnlyList<Oe1300TcpRallFrameSummary> frameSummaries, int firstFrameIndex) =>
        (frameSummaries[firstFrameIndex].Mean + frameSummaries[firstFrameIndex + 1].Mean) / 2.0;

    private static double ReadBigEndianDouble(byte[] payload, int offset)
    {
        var bits = BinaryPrimitives.ReadInt64BigEndian(payload.AsSpan(offset, sizeof(long)));
        return BitConverter.Int64BitsToDouble(bits);
    }

    private static string ToHexPrefix(byte[] payload, int count) =>
        Convert.ToHexString(payload, 0, Math.Min(count, payload.Length)).ToLowerInvariant();

    private static string ToHexSuffix(byte[] payload, int count)
    {
        if (payload.Length == 0)
        {
            return string.Empty;
        }

        var start = Math.Max(0, payload.Length - count);
        return Convert.ToHexString(payload, start, payload.Length - start).ToLowerInvariant();
    }
}

public static class Oe1300Serial
{
    public static Oe1300SerialSession Open(string portName, int baudRate) => new(portName, baudRate);
}

public sealed class Oe1300SerialSession : IDisposable
{
    private readonly SerialPort port;

    public Oe1300SerialSession(string portName, int baudRate)
    {
        port = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
        {
            Handshake = Handshake.None,
            DtrEnable = true,
            ReadTimeout = Oe1300Defaults.SerialTimeoutMs,
            WriteTimeout = Oe1300Defaults.SerialTimeoutMs,
            NewLine = "\r"
        };
        port.Open();
        Thread.Sleep(Oe1300Defaults.SerialOpenSettleMs);
        Clear();
    }

    public void Clear()
    {
        port.DiscardInBuffer();
        port.DiscardOutBuffer();
    }

    public void SendAsciiCommand(string command)
    {
        port.Write(command + "\r");
    }

    public string QueryAsciiLine(string command, int maxBytes = Oe1300Defaults.SerialAsciiMaxBytes)
    {
        Clear();
        SendAsciiCommand(command);
        return ReadAsciiLine(maxBytes);
    }

    public string QueryIdn() => QueryAsciiLine(Oe1300Commands.QueryIdn);

    public Oe1300RallSnapshot QueryRall() => Oe1300Parsers.ParseSerialRall(QueryAsciiLine(Oe1300Commands.QueryRall));

    public void Dispose() => port.Dispose();

    private string ReadAsciiLine(int maxBytes)
    {
        var buffer = new List<byte>(128);

        while (buffer.Count < maxBytes)
        {
            int value;
            try
            {
                value = port.ReadByte();
            }
            catch (TimeoutException)
            {
                if (buffer.Count == 0)
                {
                    throw new IOException("empty OE1300 serial response");
                }

                break;
            }

            if (value < 0)
            {
                if (buffer.Count == 0)
                {
                    throw new IOException("empty OE1300 serial response");
                }

                break;
            }

            if (value is 10 or 13)
            {
                if (buffer.Count == 0)
                {
                    continue;
                }

                break;
            }

            buffer.Add((byte)value);
        }

        if (buffer.Count >= maxBytes)
        {
            throw new IOException($"OE1300 serial ASCII response exceeds {maxBytes} bytes");
        }

        return Encoding.ASCII.GetString(buffer.ToArray()).Trim();
    }
}

public static class Oe1300Tcp
{
    public static Oe1300TcpSession Open(string host, int port) => new(host, port);
}

public sealed class Oe1300TcpSession : IDisposable
{
    private static readonly byte[] RallCommandBytes = Encoding.ASCII.GetBytes(Oe1300Commands.QueryRall + "\r");

    private readonly TcpClient client;
    private readonly NetworkStream stream;

    public Oe1300TcpSession(string host, int port)
    {
        client = new TcpClient
        {
            ReceiveTimeout = Oe1300Defaults.TcpReadTimeoutMs,
            SendTimeout = Oe1300Defaults.TcpReadTimeoutMs,
            NoDelay = true,
            ReceiveBufferSize = Oe1300Defaults.TcpRallExpectedBytes,
            SendBufferSize = 1024
        };

        var connectTask = client.ConnectAsync(host, port);
        if (!connectTask.Wait(Oe1300Defaults.TcpConnectTimeoutMs))
        {
            client.Dispose();
            throw new TimeoutException($"OE1300 TCP connect timeout: host={host}, port={port}, timeout_ms={Oe1300Defaults.TcpConnectTimeoutMs}");
        }

        if (connectTask.IsFaulted)
        {
            client.Dispose();
            throw connectTask.Exception?.GetBaseException() ?? new IOException($"OE1300 TCP connect failed: host={host}, port={port}");
        }

        stream = client.GetStream();
    }

    public void SendAsciiCommand(string command)
    {
        var payload = Encoding.ASCII.GetBytes(command + "\r");
        stream.Write(payload, 0, payload.Length);
    }

    public void SendRallCommand()
    {
        stream.Write(RallCommandBytes, 0, RallCommandBytes.Length);
    }

    public string QueryAsciiLine(string command, int maxBytes = Oe1300Defaults.TcpAsciiMaxBytes)
    {
        DrainAvailable();
        SendAsciiCommand(command);

        var buffer = new List<byte>(128);
        var scratch = new byte[1];

        while (buffer.Count < maxBytes)
        {
            int read;
            try
            {
                read = stream.Read(scratch, 0, 1);
            }
            catch (IOException)
            {
                if (buffer.Count == 0)
                {
                    throw new IOException("empty OE1300 TCP ASCII response");
                }

                break;
            }

            if (read == 0)
            {
                if (buffer.Count == 0)
                {
                    throw new IOException("empty OE1300 TCP ASCII response");
                }

                break;
            }

            var value = scratch[0];
            if (value is 10 or 13)
            {
                if (buffer.Count == 0)
                {
                    continue;
                }

                break;
            }

            buffer.Add(value);
        }

        if (buffer.Count >= maxBytes)
        {
            throw new IOException($"OE1300 TCP ASCII response exceeds {maxBytes} bytes");
        }

        return Encoding.ASCII.GetString(buffer.ToArray()).Trim();
    }

    public string QueryIdn() => QueryAsciiLine(Oe1300Commands.QueryIdn);

    public byte[] QueryRallBinary(
        int expectedBytes = Oe1300Defaults.TcpRallExpectedBytes,
        int postWriteDelayMs = 5,
        int idleBreakMs = 100,
        int maxWaitMs = 1500)
    {
        var payload = new byte[expectedBytes];
        var bytesRead = ReadRallFrame(payload, expectedBytes, postWriteDelayMs, idleBreakMs, maxWaitMs);
        if (bytesRead == payload.Length)
        {
            return payload;
        }

        return payload[..bytesRead];
    }

    public int ReadRallFrame(
        byte[] destination,
        int expectedBytes = Oe1300Defaults.TcpRallExpectedBytes,
        int postWriteDelayMs = 5,
        int idleBreakMs = 100,
        int maxWaitMs = 1500)
    {
        if (destination.Length < expectedBytes)
        {
            throw new ArgumentException($"destination buffer too small: required={expectedBytes}, actual={destination.Length}", nameof(destination));
        }

        DrainAvailable();
        SendRallCommand();
        if (postWriteDelayMs > 0)
        {
            Thread.Sleep(postWriteDelayMs);
        }

        var bytesReadTotal = 0;
        var started = Environment.TickCount64;
        var lastDataAt = started;

        while (Environment.TickCount64 - started < maxWaitMs)
        {
            try
            {
                var read = stream.Read(destination, bytesReadTotal, expectedBytes - bytesReadTotal);
                if (read > 0)
                {
                    bytesReadTotal += read;
                    lastDataAt = Environment.TickCount64;

                    if (bytesReadTotal >= expectedBytes)
                    {
                        break;
                    }

                    continue;
                }
            }
            catch (IOException)
            {
                if (bytesReadTotal > 0 && Environment.TickCount64 - lastDataAt >= idleBreakMs)
                {
                    break;
                }

                continue;
            }

            if (bytesReadTotal > 0 && Environment.TickCount64 - lastDataAt >= idleBreakMs)
            {
                break;
            }
        }

        if (bytesReadTotal == 0)
        {
            throw new IOException("empty OE1300 TCP RALL response");
        }

        return bytesReadTotal;
    }

    public void Dispose()
    {
        stream.Dispose();
        client.Dispose();
    }

    private void DrainAvailable()
    {
        if (!stream.DataAvailable)
        {
            return;
        }

        var scratch = new byte[4096];
        while (stream.DataAvailable)
        {
            var read = stream.Read(scratch, 0, scratch.Length);
            if (read == 0)
            {
                break;
            }
        }
    }
}
