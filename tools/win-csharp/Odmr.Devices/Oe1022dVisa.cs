using System.Text;
using Ivi.Visa;

namespace Odmr.Devices;

public static class Oe1022dDefaults
{
    public const string Resource = "ASRL8::INSTR";
    public const int BaudRate = 921600;
    public const int RallFrameBytes = 12288;
    public const int DevicePacketCounterOffset = 12287;
    public const int RallPostWriteDelayMs = 30;
    public const int VisaTimeoutMs = 300;
    public const string RequiredIdnPrefix = "SSI LIA-OE1022D";
    public const string RequiredSerial = "SN:D6522078";
}

public static class Oe1022dCommands
{
    public static readonly byte[] IdnQueryBytes = Encoding.ASCII.GetBytes("*IDN?\r");
    public static readonly byte[] RallQueryBytes = Encoding.ASCII.GetBytes("RALL?\r");
}

public static class Oe1022dVisa
{
    public static IEnumerable<string> ListResources() => GlobalResourceManager.Find();

    public static Oe1022dVisaSession Open(string resourceName, int baudRate) =>
        new(resourceName, baudRate);
}

public sealed class Oe1022dVisaSession : IDisposable
{
    private readonly IVisaSession resource;
    private readonly IMessageBasedSession session;

    public Oe1022dVisaSession(string resourceName, int baudRate)
    {
        resource = GlobalResourceManager.Open(resourceName);
        resource.TimeoutMilliseconds = Oe1022dDefaults.VisaTimeoutMs;

        if (resource is not IMessageBasedSession messageSession)
        {
            resource.Dispose();
            throw new InvalidOperationException($"resource is not message-based: {resourceName}");
        }

        session = messageSession;
        session.TerminationCharacterEnabled = false;

        if (resource is ISerialSession serial)
        {
            serial.BaudRate = baudRate;
            serial.DataBits = 8;
            serial.Parity = SerialParity.None;
            serial.StopBits = SerialStopBitsMode.One;
            serial.FlowControl = SerialFlowControlModes.None;
            serial.ReadTermination = SerialTerminationMethod.None;
            serial.WriteTermination = SerialTerminationMethod.None;
            serial.Flush(IOBuffers.Read, true);
        }
    }

    public string QueryIdn()
    {
        session.RawIO.Write(Oe1022dCommands.IdnQueryBytes);
        return ReadAsciiLine(4096);
    }

    public void WriteRallQuery() => session.RawIO.Write(Oe1022dCommands.RallQueryBytes);

    public long ReadRallFrame(byte[] payload)
    {
        session.RawIO.Read(payload, 0, payload.Length, out var bytesRead, out _);
        return bytesRead;
    }

    public void Dispose() => resource.Dispose();

    private string ReadAsciiLine(int maxBytes)
    {
        var buffer = new List<byte>(128);

        while (buffer.Count < maxBytes)
        {
            var chunk = session.RawIO.Read(1);
            if (chunk.Length == 0)
            {
                if (buffer.Count == 0)
                {
                    throw new IOException("empty ASCII response");
                }

                break;
            }

            var value = chunk[0];
            if (value is 10 or 13)
            {
                break;
            }

            buffer.Add(value);
        }

        if (buffer.Count >= maxBytes)
        {
            throw new IOException($"ASCII response exceeds {maxBytes} bytes");
        }

        return Encoding.ASCII.GetString(buffer.ToArray()).Trim();
    }
}
