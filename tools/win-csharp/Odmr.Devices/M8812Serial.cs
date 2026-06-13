using System.Globalization;
using System.IO.Ports;
using System.Text.Json.Serialization;

namespace Odmr.Devices;

public static class M8812Defaults
{
    public const string XPort = "COM4";
    public const string YPort = "COM6";
    public const string ZPort = "COM3";
    public const int BaudRate = 9600;
    public const int TimeoutMs = 300;
    public const int OpenSettleMs = 100;
    public const string XSerial = "080020960220402020";
    public const string YSerial = "080020960220402022";
    public const string ZSerial = "080020960220402003";
}

public static class M8812Commands
{
    public const string QueryIdn = "*IDN?";
    public const string SetRemote = "SYST:REM";
    public const string SetLocal = "SYST:LOC";
    public const string QueryCurrent = "MEAS:CURR?";
    public const string SetCurrentZero = "CURR 0.00000";
    public const string SetOutputOff = "OUTP 0";
    public const string SetOutputOn = "OUTP 1";

    public static string SetVoltage(double volts) => $"VOLT {volts.ToString(CultureInfo.InvariantCulture)}";

    public static string SetVoltageProtection(double volts) => $"VOLT:PROT {volts.ToString(CultureInfo.InvariantCulture)}";

    public static string SetCurrent(double amps) => $"CURR {amps.ToString("0.00000", CultureInfo.InvariantCulture)}";
}

public sealed record M8812ProbeSummary(
    [property: JsonPropertyName("axes")] IReadOnlyList<M8812AxisProbeResult> Axes,
    [property: JsonPropertyName("all_identity_matched")] bool AllIdentityMatched,
    [property: JsonPropertyName("cleanup_ok")] bool CleanupOk);

public sealed record M8812AxisProbeResult(
    [property: JsonPropertyName("axis")] string Axis,
    [property: JsonPropertyName("port")] string Port,
    [property: JsonPropertyName("expected_serial")] string ExpectedSerial,
    [property: JsonPropertyName("idn")] string Idn,
    [property: JsonPropertyName("identity_matched")] bool IdentityMatched,
    [property: JsonPropertyName("measured_current_a")] double? MeasuredCurrentA,
    [property: JsonPropertyName("cleanup_ok")] bool CleanupOk,
    [property: JsonPropertyName("error")] string? Error);

public static class M8812Serial
{
    public static M8812ProbeSummary Probe(string xPort, string yPort, string zPort)
    {
        var axes = new[]
        {
            ProbeAxis("mag_x", xPort, M8812Defaults.XSerial),
            ProbeAxis("mag_y", yPort, M8812Defaults.YSerial),
            ProbeAxis("mag_z", zPort, M8812Defaults.ZSerial)
        };

        return new M8812ProbeSummary(
            axes,
            axes.All(axis => axis.IdentityMatched),
            axes.All(axis => axis.CleanupOk));
    }

    private static M8812AxisProbeResult ProbeAxis(string axis, string portName, string expectedSerial)
    {
        using var session = Open(portName);
        var cleanupOk = false;
        string idn = "";
        double? measuredCurrent = null;

        try
        {
            session.Clear();
            idn = session.QueryIdn();
            var identityMatched = idn.Contains("MAYNUO", StringComparison.Ordinal) &&
                idn.Contains("M8812", StringComparison.Ordinal) &&
                idn.Contains(expectedSerial, StringComparison.Ordinal);

            session.SetRemote();
            measuredCurrent = session.MeasureCurrent();
            session.Cleanup();
            cleanupOk = true;

            return new M8812AxisProbeResult(
                axis,
                portName,
                expectedSerial,
                idn,
                identityMatched,
                measuredCurrent,
                cleanupOk,
                null);
        }
        catch (Exception ex)
        {
            try
            {
                session.Cleanup();
                cleanupOk = true;
            }
            catch
            {
                cleanupOk = false;
            }

            return new M8812AxisProbeResult(
                axis,
                portName,
                expectedSerial,
                idn,
                idn.Contains(expectedSerial, StringComparison.Ordinal),
                measuredCurrent,
                cleanupOk,
                ex.Message);
        }
    }

    public static M8812AxisSession Open(string portName) => new(portName);

    private static SerialPort OpenPort(string portName)
    {
        var port = new SerialPort(portName, M8812Defaults.BaudRate, Parity.None, 8, StopBits.One)
        {
            Handshake = Handshake.None,
            DtrEnable = true,
            ReadTimeout = M8812Defaults.TimeoutMs,
            WriteTimeout = M8812Defaults.TimeoutMs,
            NewLine = "\n"
        };
        port.Open();
        Thread.Sleep(M8812Defaults.OpenSettleMs);
        port.DiscardInBuffer();
        port.DiscardOutBuffer();
        return port;
    }

    internal static void Send(SerialPort port, string command)
    {
        port.Write(command + "\n");
    }

    internal static string Query(SerialPort port, string command)
    {
        Send(port, command);
        return port.ReadLine().Trim();
    }

    internal static bool Cleanup(SerialPort port)
    {
        Send(port, M8812Commands.SetCurrentZero);
        Send(port, M8812Commands.SetOutputOff);
        Thread.Sleep(100);
        Send(port, M8812Commands.SetLocal);
        return true;
    }

    private static double ParseCurrent(string response) =>
        double.Parse(response.Trim(), CultureInfo.InvariantCulture);
}

public sealed class M8812AxisSession : IDisposable
{
    private readonly SerialPort port;

    public M8812AxisSession(string portName)
    {
        port = new SerialPort(portName, M8812Defaults.BaudRate, Parity.None, 8, StopBits.One)
        {
            Handshake = Handshake.None,
            DtrEnable = true,
            ReadTimeout = M8812Defaults.TimeoutMs,
            WriteTimeout = M8812Defaults.TimeoutMs,
            NewLine = "\n"
        };
        port.Open();
        Thread.Sleep(M8812Defaults.OpenSettleMs);
        Clear();
    }

    public void Clear()
    {
        port.DiscardInBuffer();
        port.DiscardOutBuffer();
    }

    public string QueryIdn() => M8812Serial.Query(port, M8812Commands.QueryIdn);

    public void SetRemote() => M8812Serial.Send(port, M8812Commands.SetRemote);

    public void SetLocal() => M8812Serial.Send(port, M8812Commands.SetLocal);

    public void SetVoltage(double volts) => M8812Serial.Send(port, M8812Commands.SetVoltage(volts));

    public void SetVoltageProtection(double volts) => M8812Serial.Send(port, M8812Commands.SetVoltageProtection(volts));

    public void SetCurrent(double amps) => M8812Serial.Send(port, M8812Commands.SetCurrent(amps));

    public void SetOutput(bool enabled) => M8812Serial.Send(port, enabled ? M8812Commands.SetOutputOn : M8812Commands.SetOutputOff);

    public double MeasureCurrent() =>
        double.Parse(M8812Serial.Query(port, M8812Commands.QueryCurrent), CultureInfo.InvariantCulture);

    public void Cleanup() => M8812Serial.Cleanup(port);

    public void Dispose() => port.Dispose();
}
