using System.Globalization;
using System.Text;
using System.Text.Json;
using Odmr.Artifacts;
using Odmr.Runtime;

namespace Odmr.ControlPanel.WinForms;

internal sealed record ConfigSelection(
    string StationPath,
    string CalibrationPath,
    string SourcePlanPath,
    string SourceSmbProfilePath,
    string SourceOeProfilePath,
    string SourceLaserProfilePath);

internal sealed record DraftPaths(
    string PlanPath,
    string SmbProfilePath,
    string OeProfilePath,
    string LaserProfilePath,
    string DiffText);

internal sealed record AxisDraft(bool UseExplicitList, string ExplicitList, double Start, double Stop, double Step)
{
    public IReadOnlyList<double> BuildValues()
    {
        if (UseExplicitList)
        {
            var values = ExplicitList
                .Split(new[] { ',', ';', '\n', '\r', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => double.Parse(value, CultureInfo.InvariantCulture))
                .ToArray();
            if (values.Length == 0)
            {
                throw new InvalidOperationException("axis explicit list must contain at least one value");
            }

            return values;
        }

        if (Step == 0.0)
        {
            throw new InvalidOperationException("axis step must not be zero");
        }

        if (Math.Abs(Stop - Start) < 1e-12)
        {
            return new[] { Start };
        }

        if (Math.Sign(Stop - Start) != Math.Sign(Step))
        {
            throw new InvalidOperationException("axis step direction must move from start toward stop");
        }

        var valuesOut = new List<double>();
        var value = Start;
        var guard = 0;
        while ((Step > 0 && value <= Stop + 1e-9) || (Step < 0 && value >= Stop - 1e-9))
        {
            valuesOut.Add(Math.Round(value, 9));
            value += Step;
            guard++;
            if (guard > 100_000)
            {
                throw new InvalidOperationException("axis generated too many values");
            }
        }

        if (valuesOut.Count == 0)
        {
            throw new InvalidOperationException("axis generated no values");
        }

        return valuesOut;
    }
}

internal sealed record PlanDraft(
    string RunId,
    string Operator,
    AxisDraft X,
    AxisDraft Y,
    AxisDraft Z,
    string CycleMode,
    int TotalPoints,
    double BaselineXA,
    double BaselineYA,
    double BaselineZA,
    int BaselineSettleMs,
    double VoltageV,
    double VoltageProtectionV,
    bool MagOutputEnabled);

internal sealed record ProfileDraft(
    double SmbStartHz,
    double SmbStopHz,
    double SmbStepHz,
    int SmbDwellMs,
    double SmbPowerDbm,
    bool SmbRfOutputEnabled,
    int OeTimeConstantIndex,
    int OeFilterSlope);

internal sealed record DraftResult(DraftPaths Paths, ConfigResolutionSummary Resolution, string OutDir, long? EstimatedRunDurationMs);

internal sealed record RunLaunchSnapshot(
    RuntimeState State,
    string EventName,
    string Message,
    string? PointId,
    int? PointIndex,
    int? PointsTotal,
    long? FramesTotal,
    long? TimeoutCount,
    long? RawLenBadCount,
    long? DeltaGt1Count,
    string? QualityStatus);

internal sealed class ConfigCatalogService
{
    public string RepoRoot { get; }

    public ConfigCatalogService(string? startDirectory = null)
    {
        RepoRoot = FindRepoRoot(startDirectory ?? Environment.CurrentDirectory);
    }

    public IReadOnlyList<string> Stations() => Files("configs", "stations", "*.json");

    public IReadOnlyList<string> Calibrations() => Files("configs", "calibrations", "*.json");

    public IReadOnlyList<string> Plans() => Files("configs", "plans", "*.json");

    public IReadOnlyList<string> Profiles(string contains) => Files("configs", "profiles", $"*{contains}*.json");

    public string DefaultOutputRoot()
    {
        var path = Path.Combine(RepoRoot, "runs");
        Directory.CreateDirectory(path);
        return path;
    }

    private IReadOnlyList<string> Files(params string[] parts)
    {
        var pattern = parts[^1];
        var dirParts = new string[parts.Length];
        dirParts[0] = RepoRoot;
        Array.Copy(parts, 0, dirParts, 1, parts.Length - 1);
        var dir = Path.Combine(dirParts);
        return Directory.Exists(dir)
            ? Directory.GetFiles(dir, pattern).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray()
            : Array.Empty<string>();
    }

    private static string FindRepoRoot(string startDirectory)
    {
        var dir = new DirectoryInfo(startDirectory);
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "configs")) &&
                Directory.Exists(Path.Combine(dir.FullName, "tools", "win-csharp")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("could not find repository root containing configs and tools/win-csharp");
    }
}

internal sealed class DraftConfigService
{
    private static readonly JsonSerializerOptions JsonReadOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };

    public DraftResult SaveDrafts(
        ConfigSelection selection,
        PlanDraft planDraft,
        ProfileDraft profileDraft,
        string repoRoot,
        string outputRoot)
    {
        var sourcePlan = ReadJson<AcquisitionRunPlan>(selection.SourcePlanPath);
        var sourceSmb = ReadJson<Smb100aRunProfile>(selection.SourceSmbProfilePath);
        var sourceOe = ReadJson<Oe1022dRunProfile>(selection.SourceOeProfilePath);

        var generatedDir = Path.Combine(repoRoot, "configs", "generated", DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
        Directory.CreateDirectory(generatedDir);

        var runId = SanitizeId(planDraft.RunId);
        var profileSuffix = DateTime.Now.ToString("HHmmss", CultureInfo.InvariantCulture);
        var plan = BuildPlan(sourcePlan, planDraft, runId);
        var smb = BuildSmbProfile(sourceSmb, profileDraft, $"{sourceSmb.ProfileId}_ui_{profileSuffix}");
        var oe = BuildOeProfile(sourceOe, profileDraft, $"{sourceOe.ProfileId}_ui_{profileSuffix}");

        RunConfigLoader.ValidateOeCollector(oe);

        var planPath = Path.Combine(generatedDir, $"{runId}.plan.json");
        var smbPath = Path.Combine(generatedDir, $"{smb.ProfileId}.json");
        var oePath = Path.Combine(generatedDir, $"{oe.ProfileId}.json");
        var laserPath = selection.SourceLaserProfilePath;

        WriteJson(planPath, plan);
        WriteJson(smbPath, smb);
        WriteJson(oePath, oe);

        var bundle = RunConfigLoader.Load(
            selection.StationPath,
            selection.CalibrationPath,
            planPath,
            smbPath,
            oePath,
            laserPath);
        var outDir = UniqueRunDirectory(outputRoot, runId);
        var diffText = BuildSimpleDiff("PLAN", selection.SourcePlanPath, planPath) +
            Environment.NewLine +
            BuildSimpleDiff("SMB", selection.SourceSmbProfilePath, smbPath) +
            Environment.NewLine +
            BuildSimpleDiff("OE", selection.SourceOeProfilePath, oePath);

        return new DraftResult(
            new DraftPaths(planPath, smbPath, oePath, laserPath, diffText),
            bundle.ToSummary(),
            outDir,
            bundle.ResolvedPlan.EstimatedRunDurationMs);
    }

    private static AcquisitionRunPlan BuildPlan(AcquisitionRunPlan source, PlanDraft draft, string runId)
    {
        var x = draft.X.BuildValues();
        var y = draft.Y.BuildValues();
        var z = draft.Z.BuildValues();
        if (draft.CycleMode == "bounce_1d_x" && (y.Count != 1 || z.Count != 1))
        {
            throw new InvalidOperationException("bounce_1d_x requires single Y and Z values");
        }

        var basePointCount = draft.CycleMode == "bounce_1d_x"
            ? Bounce1dXCount(x.Count)
            : x.Count * y.Count * z.Count;
        var totalPoints = draft.TotalPoints > 0 ? draft.TotalPoints : basePointCount;

        return source with
        {
            RunId = runId,
            Operator = string.IsNullOrWhiteSpace(draft.Operator) ? source.Operator : draft.Operator,
            MagBaselinePolicy = source.MagBaselinePolicy with
            {
                BaselineCurrentA = new[] { draft.BaselineXA, draft.BaselineYA, draft.BaselineZA },
                SettleMs = draft.BaselineSettleMs,
                VoltageV = draft.VoltageV,
                VoltageProtectionV = draft.VoltageProtectionV,
                OutputEnabled = draft.MagOutputEnabled
            },
            PointSource = new PointSourceConfig(
                "cartesian_grid",
                new CartesianGridAxesNt(x, y, z),
                new[] { "x", "y", "z" },
                draft.CycleMode,
                new CartesianGridStopCondition("fixed_total_points", totalPoints)),
            Points = Array.Empty<RunPointPlan>()
        };
    }

    private static int Bounce1dXCount(int xCount) => xCount <= 1 ? xCount : xCount + Math.Max(0, xCount - 2);

    private static Smb100aRunProfile BuildSmbProfile(Smb100aRunProfile source, ProfileDraft draft, string profileId) =>
        source with
        {
            ProfileId = SanitizeId(profileId),
            DefaultSweep = source.DefaultSweep with
            {
                StartHz = draft.SmbStartHz,
                StopHz = draft.SmbStopHz,
                StepHz = draft.SmbStepHz,
                DwellMs = draft.SmbDwellMs,
                PowerDbm = draft.SmbPowerDbm,
                RfOutputEnabled = draft.SmbRfOutputEnabled
            }
        };

    private static Oe1022dRunProfile BuildOeProfile(Oe1022dRunProfile source, ProfileDraft draft, string profileId) =>
        source with
        {
            ProfileId = SanitizeId(profileId),
            Fixed = source.Fixed with
            {
                TimeConstantIndex = draft.OeTimeConstantIndex,
                FilterSlope = draft.OeFilterSlope
            }
        };

    private static T ReadJson<T>(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json, JsonReadOptions) ??
            throw new InvalidOperationException($"failed to parse JSON: {path}");
    }

    private static void WriteJson<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(value, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));
    }

    private static string UniqueRunDirectory(string outputRoot, string runId)
    {
        Directory.CreateDirectory(outputRoot);
        var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        var basePath = Path.Combine(outputRoot, $"{stamp}_{runId}");
        var candidate = basePath;
        var index = 1;
        while (Directory.Exists(candidate))
        {
            candidate = $"{basePath}_{index:00}";
            index++;
        }

        return candidate;
    }

    private static string BuildSimpleDiff(string title, string sourcePath, string generatedPath)
    {
        var source = File.Exists(sourcePath) ? File.ReadAllLines(sourcePath) : Array.Empty<string>();
        var generated = File.Exists(generatedPath) ? File.ReadAllLines(generatedPath) : Array.Empty<string>();
        var builder = new StringBuilder();
        builder.AppendLine($"[{title}] {Path.GetFileName(sourcePath)} -> {Path.GetFileName(generatedPath)}");
        var max = Math.Max(source.Length, generated.Length);
        for (var i = 0; i < max; i++)
        {
            var left = i < source.Length ? source[i] : null;
            var right = i < generated.Length ? generated[i] : null;
            if (left == right)
            {
                continue;
            }

            if (left is not null)
            {
                builder.AppendLine($"- {left}");
            }
            if (right is not null)
            {
                builder.AppendLine($"+ {right}");
            }
        }

        return builder.ToString();
    }

    private static string SanitizeId(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value.Trim())
        {
            builder.Append(char.IsLetterOrDigit(ch) || ch is '_' or '-' ? ch : '_');
        }

        return builder.Length == 0 ? $"ui_run_{DateTime.Now:yyyyMMdd_HHmmss}" : builder.ToString();
    }
}

internal sealed class RunLaunchService
{
    private CancellationTokenSource? cancellation;

    public bool IsRunning { get; private set; }

    public void RequestStopAfterCurrentPoint() => cancellation?.Cancel();

    public async Task<RunSummaryRecord> RunAsync(
        ConfigSelection selection,
        DraftPaths draftPaths,
        string outDir,
        IProgress<RunLaunchSnapshot> progress)
    {
        if (IsRunning)
        {
            throw new InvalidOperationException("a run is already active");
        }

        if (Directory.Exists(outDir))
        {
            throw new InvalidOperationException($"out-dir already exists: {outDir}");
        }

        using var runCancellation = new CancellationTokenSource();
        cancellation = runCancellation;
        IsRunning = true;
        try
        {
            var runtimeProgress = new Progress<RunProgressEvent>(evt =>
            {
                progress.Report(new RunLaunchSnapshot(
                    evt.State,
                    evt.EventName,
                    evt.Message,
                    evt.PointId,
                    evt.PointIndex,
                    evt.PointsTotal,
                    evt.FramesTotal,
                    evt.TimeoutCount,
                    evt.RawLenBadCount,
                    evt.DeltaGt1Count,
                    evt.QualityStatus));
            });

            return await Task.Run(() => ConfigDrivenRun.Execute(new ConfigDrivenRunOptions(
                selection.StationPath,
                selection.CalibrationPath,
                draftPaths.PlanPath,
                draftPaths.SmbProfilePath,
                draftPaths.OeProfilePath,
                draftPaths.LaserProfilePath,
                outDir,
                runtimeProgress,
                runCancellation.Token)));
        }
        finally
        {
            IsRunning = false;
            cancellation = null;
        }
    }
}
