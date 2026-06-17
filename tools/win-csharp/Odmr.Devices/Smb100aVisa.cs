using System.Diagnostics;
using System.Text;
using Ivi.Visa;

namespace Odmr.Devices;

public static class Smb100aVisa
{
    public static IEnumerable<string> ListResources() =>
        GlobalResourceManager.Find()
            .Where(resource =>
                resource.StartsWith("USB::", StringComparison.OrdinalIgnoreCase) ||
                resource.StartsWith("USB0::", StringComparison.OrdinalIgnoreCase));

    public static SmbProbeSummary Probe(string resource)
    {
        using var session = Open(resource);
        var idn = session.Query(Smb100aCommands.QueryIdn);
        var error = session.Query(Smb100aCommands.QuerySystemError);
        var output = session.Query(Smb100aCommands.QueryOutput);

        return new SmbProbeSummary(
            "visa_resource",
            null,
            null,
            resource,
            idn,
            error,
            output,
            idn.Contains(Smb100aDefaults.RequiredVendor, StringComparison.Ordinal) &&
                idn.Contains(Smb100aDefaults.RequiredModel, StringComparison.Ordinal),
            Smb100aTcp.ErrorIsClean(error));
    }

    public static Smb100aVisaSession Open(string resource) => new(resource);
}

public sealed class Smb100aVisaSession : ISmb100aSession
{
    private readonly IVisaSession resource;
    private readonly IMessageBasedSession session;

    public Smb100aVisaSession(string resourceName)
    {
        resource = GlobalResourceManager.Open(resourceName);
        resource.TimeoutMilliseconds = Smb100aDefaults.TimeoutMs;

        if (resource is not IMessageBasedSession messageSession)
        {
            resource.Dispose();
            throw new InvalidOperationException($"resource is not message-based: {resourceName}");
        }

        session = messageSession;
        session.TerminationCharacterEnabled = false;
    }

    public void Send(string command)
    {
        session.RawIO.Write(Encoding.ASCII.GetBytes(command + "\n"));
    }

    public string Query(string command)
    {
        session.RawIO.Write(Encoding.ASCII.GetBytes(command + "\n"));
        return ReadAsciiLine(4096);
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

    public void Cleanup()
    {
        Send(Smb100aCommands.OutputOff);
        Send(Smb100aCommands.FrequencyModeCw);
        Thread.Sleep(500);
        EnsureNoError();
    }

    public void Dispose()
    {
        resource.Dispose();
    }

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
            throw new IOException($"ASCII response exceeds {maxBytes} bytes");
        }

        return Encoding.ASCII.GetString(buffer.ToArray()).Trim();
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
}
