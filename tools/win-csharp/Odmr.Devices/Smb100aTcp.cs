using System.Diagnostics;
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
    public const string QueryOperationComplete = "*OPC?";
    public const string QueryOutput = "OUTP?";
    public const string QueryFrequencyMode = "FREQ:MODE?";

    public const string FrequencyModeSweep = "FREQ:MODE SWE";
    public const string FrequencyModeCw = "FREQ:MODE CW";
    public const string ExecuteFrequencySweep = "SWE:FREQ:EXEC";
    public const string OutputOn = "OUTP ON";
    public const string OutputOff = "OUTP OFF";

    public static string SetFrequencyHz(long hz) => $"FREQ {hz}";

    public static readonly string[] FixedSweepProfile =
    [
        "MOD:STAT ON",
        "FM:STAT ON",
        "FM:SOUR INT",
        "FM:MODE HDEV",
        "FM:DEV 4000000Hz",
        "LFO ON",
        "LFO:VOLT 137mV",
        "LFO:FREQ 500Hz",
        "LFO:SHAP SQU",
        "SOUR:LFO:SIMP LOW"
    ];

    public static readonly string[] DefaultSweepProfile =
    [
        FrequencyModeSweep,
        "POW -10dBm",
        "FREQ:STAR 2830000000Hz",
        "FREQ:STOP 2890000000Hz",
        "SWE:FREQ:STEP 500000Hz",
        "SWE:FREQ:DWEL 300ms",
        "SWE:MODE AUTO",
        "SWE:SPAC LIN",
        "SWE:SHAP SAWT",
        "TRIG:FSW:SOUR AUTO",
        "SWE:OVOL:STAR 0",
        "SWE:OVOL:STOP 3",
        OutputOn
    ];
}

public sealed record SmbSweepSpec(
    [property: JsonPropertyName("start_hz")] long StartHz,
    [property: JsonPropertyName("stop_hz")] long StopHz,
    [property: JsonPropertyName("step_hz")] long StepHz,
    [property: JsonPropertyName("dwell_ms")] int DwellMs,
    [property: JsonPropertyName("power_dbm")] double PowerDbm,
    [property: JsonPropertyName("sweep_mode")] string SweepMode,
    [property: JsonPropertyName("spacing")] string Spacing,
    [property: JsonPropertyName("shape")] string Shape,
    [property: JsonPropertyName("trigger_source")] string TriggerSource,
    [property: JsonPropertyName("output_voltage_start_v")] double OutputVoltageStartV,
    [property: JsonPropertyName("output_voltage_stop_v")] double OutputVoltageStopV,
    [property: JsonPropertyName("rf_output_enabled")] bool RfOutputEnabled)
{
    public static readonly SmbSweepSpec Default = new(
        2_830_000_000,
        2_890_000_000,
        500_000,
        300,
        -10.0,
        "AUTO",
        "LIN",
        "SAWT",
        "AUTO",
        0.0,
        3.0,
        true);

    public SmbSweepSpec WithPowerDbm(double powerDbm) => this with { PowerDbm = powerDbm };

    public string[] ToCommands() =>
    [
        $"POW {PowerDbm}dBm",
        $"FREQ:STAR {StartHz}Hz",
        $"FREQ:STOP {StopHz}Hz",
        $"SWE:FREQ:STEP {StepHz}Hz",
        $"SWE:FREQ:DWEL {DwellMs}ms",
        $"SWE:MODE {SweepMode}",
        $"SWE:SPAC {Spacing}",
        $"SWE:SHAP {Shape}",
        $"TRIG:FSW:SOUR {TriggerSource}",
        $"SWE:OVOL:STAR {OutputVoltageStartV}",
        $"SWE:OVOL:STOP {OutputVoltageStopV}"
    ];

    [JsonPropertyName("sweep_points")]
    public long SweepPoints => Math.Abs(StopHz - StartHz) / StepHz + 1;

    [JsonPropertyName("estimated_sweep_duration_ms")]
    public long EstimatedSweepDurationMs => SweepPoints * DwellMs;
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
        using var session = Open(host, port);
        var idn = session.Query(Smb100aCommands.QueryIdn);
        var error = session.Query(Smb100aCommands.QuerySystemError);
        var output = session.Query(Smb100aCommands.QueryOutput);

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

    public static Smb100aSession Open(string host, int port) => new(host, port);

    public static bool ErrorIsClean(string response) =>
        response.StartsWith("0,", StringComparison.Ordinal) ||
        response.Contains("No error", StringComparison.OrdinalIgnoreCase);
}

public sealed class Smb100aSession : IDisposable
{
    private readonly TcpClient client;
    private readonly NetworkStream stream;

    public Smb100aSession(string host, int port)
    {
        client = new TcpClient
        {
            ReceiveTimeout = Smb100aDefaults.TimeoutMs,
            SendTimeout = Smb100aDefaults.TimeoutMs
        };
        client.Connect(host, port);
        stream = client.GetStream();
    }

    public void Send(string command)
    {
        var payload = Encoding.ASCII.GetBytes(command + "\n");
        stream.Write(payload, 0, payload.Length);
    }

    public string Query(string command)
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

    public void SendAndCheck(string command, int settleMs)
    {
        Send(command);
        Thread.Sleep(settleMs);
        EnsureNoError();
    }

    public void EnsureNoError()
    {
        var error = Query(Smb100aCommands.QuerySystemError);
        if (!Smb100aTcp.ErrorIsClean(error))
        {
            throw new IOException($"SMB100A error queue is not clean: {error}");
        }
    }

    public void ApplyDefaultFixedProfile(int settleMs)
    {
        foreach (var command in Smb100aCommands.FixedSweepProfile)
        {
            SendAndCheck(command, settleMs);
        }
    }

    public void ConfigureDefaultSweep(int settleMs)
    {
        ConfigureSweep(SmbSweepSpec.Default, settleMs);
    }

    public void ConfigureSweep(SmbSweepSpec spec, int settleMs)
    {
        SendAndCheck(Smb100aCommands.FrequencyModeSweep, settleMs);
        foreach (var command in spec.ToCommands())
        {
            SendAndCheck(command, settleMs);
        }
        SendAndCheck(spec.RfOutputEnabled ? Smb100aCommands.OutputOn : Smb100aCommands.OutputOff, settleMs);

        var output = Query(Smb100aCommands.QueryOutput);
        var expectedOutput = spec.RfOutputEnabled ? "1" : "0";
        if (output.Trim() != expectedOutput)
        {
            throw new IOException($"SMB100A output state mismatch: expected {expectedOutput}, observed {output}");
        }

        var frequencyMode = Query(Smb100aCommands.QueryFrequencyMode);
        if (frequencyMode.Trim() != "SWE")
        {
            throw new IOException($"SMB100A frequency mode mismatch: expected SWE, observed {frequencyMode}");
        }
    }

    public SmbSweepObservation ExecuteDefaultSweep()
    {
        return ExecuteSweep(SmbSweepSpec.Default);
    }

    public SmbSweepObservation ExecuteSweep(SmbSweepSpec spec, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var started = Stopwatch.GetTimestamp();
        Send(Smb100aCommands.ExecuteFrequencySweep);
        var response = Query(Smb100aCommands.QueryOperationComplete);
        cancellationToken.ThrowIfCancellationRequested();
        if (response.Trim() != "1")
        {
            throw new IOException($"SMB100A *OPC? returned unexpected value: {response}");
        }

        var opcWaitMs = (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        var fallbackUsed = opcWaitMs + SmbSweepObservation.SweepCompletionGuardMs < spec.EstimatedSweepDurationMs;
        if (fallbackUsed)
        {
            var remainingMs = spec.EstimatedSweepDurationMs - opcWaitMs + SmbSweepObservation.SweepCompletionGuardMs;
            SleepInterruptibly((int)Math.Max(0, remainingMs), cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
        EnsureNoError();
        return new SmbSweepObservation(spec.EstimatedSweepDurationMs, opcWaitMs, fallbackUsed);
    }

    private static void SleepInterruptibly(int milliseconds, CancellationToken cancellationToken)
    {
        if (milliseconds <= 0)
        {
            return;
        }

        if (cancellationToken.WaitHandle.WaitOne(milliseconds))
        {
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    public void Cleanup()
    {
        Send(Smb100aCommands.OutputOff);
        Send(Smb100aCommands.FrequencyModeCw);
        Thread.Sleep(500);
        EnsureNoError();
    }

    public void Dispose()
    {
        stream.Dispose();
        client.Dispose();
    }
}

public sealed record SmbSweepObservation(
    [property: JsonPropertyName("estimated_sweep_duration_ms")] long EstimatedSweepDurationMs,
    [property: JsonPropertyName("opc_wait_ms")] long OpcWaitMs,
    [property: JsonPropertyName("fallback_used")] bool FallbackUsed)
{
    public const long SweepCompletionGuardMs = 100;
}
