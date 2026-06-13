using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;

namespace Odmr.Devices;

public static class Smb100aDefaults
{
    public const string Host = "169.254.2.20";
    public const int Port = 5025;
    public const int TimeoutMs = 3000;
    public const string RequiredVendor = "Rohde&Schwarz";
    public const string RequiredModel = "SMB100A";
}

public static class Smb100aCommands
{
    public const string QueryIdn = "*IDN?";
    public const string QuerySystemError = "SYST:ERR?";
    public const string QueryOutput = "OUTP?";
}

public sealed record SmbProbeSummary(
    [property: JsonPropertyName("host")]
    string Host,
    [property: JsonPropertyName("port")]
    int Port,
    [property: JsonPropertyName("idn")]
    string Idn,
    [property: JsonPropertyName("system_error")]
    string SystemError,
    [property: JsonPropertyName("output_state")]
    string OutputState,
    [property: JsonPropertyName("identity_matched")]
    bool IdentityMatched,
    [property: JsonPropertyName("error_queue_clean")]
    bool ErrorQueueClean);

public static class Smb100aTcp
{
    public static SmbProbeSummary Probe(string host, int port)
    {
        using var client = new TcpClient();
        client.ReceiveTimeout = Smb100aDefaults.TimeoutMs;
        client.SendTimeout = Smb100aDefaults.TimeoutMs;
        client.Connect(host, port);

        using var stream = client.GetStream();
        var idn = QueryAsciiLine(stream, Smb100aCommands.QueryIdn);
        var error = QueryAsciiLine(stream, Smb100aCommands.QuerySystemError);
        var output = QueryAsciiLine(stream, Smb100aCommands.QueryOutput);

        return new SmbProbeSummary(
            host,
            port,
            idn,
            error,
            output,
            idn.Contains(Smb100aDefaults.RequiredVendor, StringComparison.Ordinal) &&
                idn.Contains(Smb100aDefaults.RequiredModel, StringComparison.Ordinal),
            ErrorIsClean(error));
    }

    private static string QueryAsciiLine(NetworkStream stream, string command)
    {
        var payload = Encoding.ASCII.GetBytes(command + "\n");
        stream.Write(payload, 0, payload.Length);

        var buffer = new List<byte>(128);
        var scratch = new byte[1];

        while (buffer.Count < 4096)
        {
            var read = stream.Read(scratch, 0, 1);
            if (read == 0)
            {
                if (buffer.Count == 0)
                {
                    throw new IOException($"empty TCP response for {command}");
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

        if (buffer.Count >= 4096)
        {
            throw new IOException($"TCP response exceeds 4096 bytes for {command}");
        }

        return Encoding.ASCII.GetString(buffer.ToArray()).Trim();
    }

    private static bool ErrorIsClean(string response) =>
        response.StartsWith("0,", StringComparison.Ordinal) ||
        response.Contains("No error", StringComparison.OrdinalIgnoreCase);
}
