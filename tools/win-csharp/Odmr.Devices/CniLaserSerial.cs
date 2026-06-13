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
    public static readonly byte[] OutputOff = [0x55, 0xAA, 0x03, 0x00, 0x03];
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
