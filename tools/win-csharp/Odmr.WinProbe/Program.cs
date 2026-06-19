namespace Odmr.WinProbe;

internal static class Program
{
    internal static int Main(string[] args)
    {
        try
        {
            return Run(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    internal static int Run(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
        {
            PrintUsage();
            return args.Length == 0 ? 1 : 0;
        }

        var command = args[0];
        var options = CliSupport.ParseOptions(args.Skip(1).ToArray());

        if (RunCliCommands.TryExecute(command, options, out var runExitCode))
        {
            return runExitCode;
        }

        if (DiagnosticsCliCommands.TryExecute(command, options, out var diagnosticsExitCode))
        {
            return diagnosticsExitCode;
        }

        return CliSupport.Fail($"unknown command: {command}");
    }

    private static void PrintUsage()
    {
        Console.WriteLine("""
        Odmr.WinProbe

        Usage:
          Formal run / artifact commands:
            Odmr.WinProbe run-resolve --station <json> --calibration <json> --plan <json> --smb-profile <json> --oe-profile <json> --laser-profile <json>
            Odmr.WinProbe run-execute --station <json> --calibration <json> --plan <json> --smb-profile <json> --oe-profile <json> --laser-profile <json> --out-dir <dir> [--progress-jsonl <path>] [--stop-request-file <path>] [--emergency-stop-file <path>]
            Odmr.WinProbe resume-run --previous-run <dir> --out-dir <dir> [--progress-jsonl <path>] [--stop-request-file <path>] [--emergency-stop-file <path>]
            Odmr.WinProbe artifact-check --run <run-dir>
            Odmr.WinProbe audit-continuity --run <run-dir> --out <json>
            Odmr.WinProbe device-command-check
            Odmr.WinProbe live-replay --run <run-dir> [--tail-events 20]

          Diagnostics / probe / demo commands:
            Odmr.WinProbe visa-list
            Odmr.WinProbe oe-idn [--resource ASRL8::INSTR] [--baud 921600]
            Odmr.WinProbe oe-rall [--resource ASRL8::INSTR] [--baud 921600] [--in-thread-process-mode none|measurement-means|field-decode-csv] [--write-raw true|false] [--write-values true|false] [--preview-field-index 8] --duration-sec 300 --out-dir <dir>
            Odmr.WinProbe oe1300-idn --port <COMx> [--baud 115200]
            Odmr.WinProbe oe1300-rall --port <COMx> [--baud 115200] [--count 1] --out-dir <dir>
            Odmr.WinProbe oe1300-net-idn [--host 192.168.1.1] [--port 10001]
            Odmr.WinProbe oe1300-net-rall [--host 192.168.1.1] [--port 10001] [--count 1] --out-dir <dir>
            Odmr.WinProbe oe1300-net-labview-demo [--host 192.168.1.1] [--port 10001] [--post-write-delay-ms 5] [--drain-before-write true|false] [--preview-param-index 0] [--write-values true|false] [--csv-write-mode all|unique-only] --duration-sec 10 --out-dir <dir>
            Odmr.WinProbe smb-probe [--list-resources] [--resource USB0::0x0AAD::0x0054::106789::INSTR | --host 169.254.2.20 [--port 5025] | --station configs/stations/lab_a.json]
            Odmr.WinProbe smb-validate [same target options as smb-probe] [--smb-profile <json>]
            Odmr.WinProbe sweep-only-run [--resource ASRL8::INSTR] [--baud 921600] [--smb-resource USB0::0x0AAD::0x0054::106789::INSTR] [--repeat 1] --out-dir <dir>
            Odmr.WinProbe minimal-3point-run [--resource ASRL8::INSTR] [--baud 921600] [--smb-resource USB0::0x0AAD::0x0054::106789::INSTR] [--x COM4] [--y COM6] [--z COM3] [--cycles 1] [--laser-background] [--laser-port COM9] [--laser-power-mw 50] --out-dir <dir>
            Odmr.WinProbe m8812-probe [--x COM4] [--y COM6] [--z COM3]
            Odmr.WinProbe laser-probe [--port COM9] --off-only
        """);
    }
}
