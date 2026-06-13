using System.IO.Ports;
using System.Text.Json.Serialization;

namespace Odmr.Devices;

public static class CniLaserDefaults
{
    public const string Port = "COM9";
    public const int BaudRate = 9600;
    public const int TimeoutMs = 500;
}

public static class CniLaserCommands
{
    public const byte CommandSetPower = 0x05;
    public const byte CommandOutput = 0x03;
    public static readonly byte[] OutputOff = [0x55, 0xAA, 0x03, 0x00, 0x03];
    public static readonly byte[] OutputOn = [0x55, 0xAA, 0x03, 0x01, 0x04];

    public static byte[] SetPowerMw(int powerMw)
    {
        if (powerMw < 0 || powerMw > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(powerMw), "laser power must fit uint16 mW");
        }

        var hi = (byte)((powerMw >> 8) & 0xFF);
        var lo = (byte)(powerMw & 0xFF);
        var checksum = (byte)((CommandSetPower + 0x01 + hi + lo) & 0xFF);
        return [0x55, 0xAA, CommandSetPower, 0x01, hi, lo, checksum];
    }
}

public sealed record CniLaserProbeSummary(
    [property: JsonPropertyName("port")] string Port,
    [property: JsonPropertyName("command")] string Command,
    [property: JsonPropertyName("echo_hex")] string EchoHex,
    [property: JsonPropertyName("echo_matched")] bool EchoMatched);

public static class CniLaserSerial
{
    public static CniLaserProbeSummary ProbeOffOnly(string portName)
    {
        using var port = OpenPort(portName);
        port.DiscardInBuffer();
        port.DiscardOutBuffer();

        port.Write(CniLaserCommands.OutputOff, 0, CniLaserCommands.OutputOff.Length);
        var echo = ReadExact(port, CniLaserCommands.OutputOff.Length);
        var matched = echo.SequenceEqual(CniLaserCommands.OutputOff);

        return new CniLaserProbeSummary(
            portName,
            "output_off",
            Convert.ToHexString(echo),
            matched);
    }

    public static CniLaserSession Open(string portName) => new(portName);

    private static SerialPort OpenPort(string portName)
    {
        var port = new SerialPort(portName, CniLaserDefaults.BaudRate, Parity.None, 8, StopBits.One)
        {
            Handshake = Handshake.None,
            ReadTimeout = CniLaserDefaults.TimeoutMs,
            WriteTimeout = CniLaserDefaults.TimeoutMs
        };
        port.Open();
        return port;
    }

    private static byte[] ReadExact(SerialPort port, int length)
    {
        var buffer = new byte[length];
        var offset = 0;
        while (offset < length)
        {
            var read = port.Read(buffer, offset, length - offset);
            if (read == 0)
            {
                throw new IOException($"laser echo ended early at {offset}/{length} bytes");
            }

            offset += read;
        }

        return buffer;
    }
}

public sealed class CniLaserSession : IDisposable
{
    private readonly SerialPort port;

    public CniLaserSession(string portName)
    {
        port = new SerialPort(portName, CniLaserDefaults.BaudRate, Parity.None, 8, StopBits.One)
        {
            Handshake = Handshake.None,
            ReadTimeout = CniLaserDefaults.TimeoutMs,
            WriteTimeout = CniLaserDefaults.TimeoutMs
        };
        port.Open();
        port.DiscardInBuffer();
        port.DiscardOutBuffer();
    }

    public string SetPowerMw(int powerMw) => WriteFrame(CniLaserCommands.SetPowerMw(powerMw));

    public string OutputOn() => WriteFrame(CniLaserCommands.OutputOn);

    public string OutputOff() => WriteFrame(CniLaserCommands.OutputOff);

    private string WriteFrame(byte[] frame)
    {
        port.Write(frame, 0, frame.Length);
        var echo = ReadExact(frame.Length);
        if (!echo.SequenceEqual(frame))
        {
            throw new IOException($"laser echo mismatch: expected={Convert.ToHexString(frame)}, observed={Convert.ToHexString(echo)}");
        }

        return Convert.ToHexString(echo);
    }

    private byte[] ReadExact(int length)
    {
        var buffer = new byte[length];
        var offset = 0;
        while (offset < length)
        {
            var read = port.Read(buffer, offset, length - offset);
            if (read == 0)
            {
                throw new IOException($"laser echo ended early at {offset}/{length} bytes");
            }

            offset += read;
        }

        return buffer;
    }

    public void Dispose() => port.Dispose();
}
