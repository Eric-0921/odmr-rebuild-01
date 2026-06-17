using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using Odmr.Artifacts;
using Odmr.Runtime;

namespace Odmr.ControlPanel.WinForms;

internal sealed class MainForm : Form
{
    private readonly ConfigCatalogService catalog;
    private readonly RunLaunchService launcher = new();

    private ConfigPicker stationPicker = null!;
    private ConfigPicker calibrationPicker = null!;
    private ConfigPicker planPicker = null!;
    private ConfigPicker smbProfilePicker = null!;
    private ConfigPicker oeProfilePicker = null!;
    private ConfigPicker laserProfilePicker = null!;
    private TextBox outputRootBox = null!;
    private TextBox bundleSummaryBox = null!;
    private TextBox bundleDetailsBox = null!;
    private Label resolveSummaryLabel = null!;
    private Label outDirLabel = null!;
    private Label stateLabel = null!;
    private Label metricsLabel = null!;
    private ProgressBar progressBar = null!;
    private ListBox eventList = null!;
    private Button resolveButton = null!;
    private Button runButton = null!;
    private Button stopButton = null!;

    private ConfigSelection? lastSelection;
    private RunConfigBundle? lastBundle;
    private string? lastOutDir;

    public MainForm()
    {
        Text = "ODMR Control Panel";
        Width = 1180;
        Height = 820;
        MinimumSize = new Size(980, 700);
        catalog = new ConfigCatalogService();
        BuildUi();
        LoadCatalogs();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 300));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        Controls.Add(root);

        root.Controls.Add(BuildRunBundlePanel(), 0, 0);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildBundleDetailsTab());
        root.Controls.Add(tabs, 0, 1);
        root.Controls.Add(BuildRunStatusPanel(), 0, 2);
    }

    private Control BuildRunBundlePanel()
    {
        var group = new GroupBox { Text = "Run Bundle", Dock = DockStyle.Fill };
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(8) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        group.Controls.Add(root);

        var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 8 };
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(left, 0, 0);

        stationPicker = AddPicker(left, "Hardware station");
        calibrationPicker = AddPicker(left, "Field calibration");
        planPicker = AddPicker(left, "Magnetic plan");
        smbProfilePicker = AddPicker(left, "SMB100A profile");
        oeProfilePicker = AddPicker(left, "OE1022D profile");
        laserProfilePicker = AddPicker(left, "Laser profile");
        left.Controls.Add(BuildOutputRow(), 0, 6);

        var actionRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
        actionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        actionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        actionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        resolveButton = new Button { Text = "Validate Bundle", Dock = DockStyle.Fill };
        resolveButton.Click += (_, _) => ResolveBundle(showDialogOnError: true);
        actionRow.Controls.Add(resolveButton, 0, 0);
        resolveSummaryLabel = new Label { Text = "Bundle: not validated", Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };
        actionRow.Controls.Add(resolveSummaryLabel, 1, 0);
        outDirLabel = new Label { Text = "Out-dir: not generated", Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };
        actionRow.Controls.Add(outDirLabel, 2, 0);
        left.Controls.Add(actionRow, 0, 7);

        var summaryGroup = new GroupBox { Text = "Run Bundle Summary", Dock = DockStyle.Fill };
        bundleSummaryBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font(FontFamily.GenericMonospace, 9),
            Text = "Validate the bundle to preview station, plan, profiles, and output."
        };
        summaryGroup.Controls.Add(bundleSummaryBox);
        root.Controls.Add(summaryGroup, 1, 0);

        foreach (var picker in AllPickers())
        {
            picker.SelectionChanged += (_, _) => InvalidateResolvedBundle();
        }

        return group;
    }

    private ConfigPicker AddPicker(TableLayoutPanel parent, string label)
    {
        var picker = new ConfigPicker(label) { Dock = DockStyle.Fill };
        parent.Controls.Add(picker);
        return picker;
    }

    private Control BuildOutputRow()
    {
        var row = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 1 };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
        row.Controls.Add(new Label { Text = "Output root", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
        outputRootBox = new TextBox { Dock = DockStyle.Fill };
        outputRootBox.TextChanged += (_, _) => InvalidateResolvedBundle();
        row.Controls.Add(outputRootBox, 1, 0);
        var browse = new Button { Text = "Browse...", Dock = DockStyle.Fill };
        browse.Click += (_, _) => BrowseOutputRoot();
        row.Controls.Add(browse, 2, 0);
        var open = new Button { Text = "Open Dir", Dock = DockStyle.Fill };
        open.Click += (_, _) => OpenDirectory(outputRootBox.Text, createIfMissing: true);
        row.Controls.Add(open, 3, 0);
        row.Controls.Add(new Label { Text = "Run outputs", Anchor = AnchorStyles.Left, AutoSize = true }, 4, 0);
        return row;
    }

    private TabPage BuildBundleDetailsTab()
    {
        var page = new TabPage("Bundle Details");
        bundleDetailsBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            Font = new Font(FontFamily.GenericMonospace, 9),
            ReadOnly = true,
            Text = "Bundle details will appear after validation."
        };
        page.Controls.Add(bundleDetailsBox);
        return page;
    }

    private Control BuildRunStatusPanel()
    {
        var group = new GroupBox { Text = "Run Feedback", Dock = DockStyle.Fill };
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3, Padding = new Padding(8) };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        group.Controls.Add(table);

        runButton = new Button { Text = "Run", Dock = DockStyle.Fill };
        runButton.Click += async (_, _) => await RunAsync();
        stopButton = new Button { Text = "Stop After Current Point", Dock = DockStyle.Fill, Enabled = false };
        stopButton.Click += (_, _) => launcher.RequestStopAfterCurrentPoint();
        table.Controls.Add(runButton, 0, 0);
        table.Controls.Add(stopButton, 0, 1);

        stateLabel = new Label { Text = "State: idle", Dock = DockStyle.Fill, AutoEllipsis = true };
        table.Controls.Add(stateLabel, 1, 0);
        metricsLabel = new Label { Text = "Metrics: not running", Dock = DockStyle.Fill, AutoEllipsis = true };
        table.Controls.Add(metricsLabel, 1, 1);
        progressBar = new ProgressBar { Dock = DockStyle.Fill, Minimum = 0, Maximum = 100 };
        table.Controls.Add(progressBar, 1, 2);

        eventList = new ListBox { Dock = DockStyle.Fill };
        table.Controls.Add(eventList, 3, 0);
        table.SetRowSpan(eventList, 3);

        return group;
    }

    private void LoadCatalogs()
    {
        LoadPicker(stationPicker, catalog.Stations(), "lab_a.json");
        LoadPicker(calibrationPicker, catalog.Calibrations(), "main.json");
        LoadPicker(planPicker, catalog.Plans(), "x_axis_1d_bounce_15min.json");
        LoadPicker(smbProfilePicker, catalog.Profiles("smb100a"), "smb100a_run_monitor_2830_2890_-10dbm.json");
        LoadPicker(oeProfilePicker, catalog.Profiles("oe1022d"), "oe1022d_run_ch_b_observed.json");
        LoadPicker(laserProfilePicker, catalog.Profiles("cni_laser"), "cni_laser_run_off_background.json");
        outputRootBox.Text = catalog.DefaultOutputRoot();
    }

    private static void LoadPicker(ConfigPicker picker, IReadOnlyList<string> paths, string preferredName)
    {
        var preferred = paths.FirstOrDefault(path => Path.GetFileName(path).Equals(preferredName, StringComparison.OrdinalIgnoreCase));
        picker.LoadPaths(paths, preferred ?? paths.FirstOrDefault());
    }

    private bool ResolveBundle(bool showDialogOnError)
    {
        try
        {
            var selection = BuildSelection();
            ValidatePickerJson(stationPicker, selection.StationPath, RunConfigLoader.ReadJson<StationSpec>);
            ValidatePickerJson(calibrationPicker, selection.CalibrationPath, RunConfigLoader.ReadJson<CalibrationProfile>);
            ValidatePickerJson(planPicker, selection.PlanPath, RunConfigLoader.ReadJson<AcquisitionRunPlan>);
            ValidatePickerJson(smbProfilePicker, selection.SmbProfilePath, RunConfigLoader.ReadJson<Smb100aRunProfile>);
            ValidatePickerJson(oeProfilePicker, selection.OeProfilePath, RunConfigLoader.ReadJson<OeRunProfile>);
            ValidatePickerJson(laserProfilePicker, selection.LaserProfilePath, RunConfigLoader.ReadJson<LaserRunProfile>);

            var bundle = RunConfigLoader.Load(
                selection.StationPath,
                selection.CalibrationPath,
                selection.PlanPath,
                selection.SmbProfilePath,
                selection.OeProfilePath,
                selection.LaserProfilePath);
            var outDir = UniqueRunDirectory(outputRootBox.Text, bundle.Plan.RunId);

            lastSelection = selection;
            lastBundle = bundle;
            lastOutDir = outDir;

            var summary = bundle.ToSummary();
            var duration = bundle.ResolvedPlan.EstimatedRunDurationMs.HasValue
                ? TimeSpan.FromMilliseconds(bundle.ResolvedPlan.EstimatedRunDurationMs.Value).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)
                : "-";
            resolveSummaryLabel.Text = $"Bundle: {summary.RunId}, {summary.ResolvedPointCount} points, {summary.SourceKind}, est={duration}";
            outDirLabel.Text = $"Out-dir: {outDir}";
            bundleSummaryBox.Text = BuildSummaryText(bundle, outDir);
            bundleDetailsBox.Text = BuildDetailsText(selection, bundle, outDir);
            AppendLog("bundle validated");
            return true;
        }
        catch (Exception ex)
        {
            lastSelection = null;
            lastBundle = null;
            lastOutDir = null;
            resolveSummaryLabel.Text = "Bundle: validation failed";
            outDirLabel.Text = "Out-dir: not generated";
            bundleSummaryBox.Text = ex.Message;
            bundleDetailsBox.Text = ex.ToString();
            AppendLog($"bundle validation failed: {ex.Message}");
            if (showDialogOnError)
            {
                MessageBox.Show(this, ex.Message, "Bundle validation failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }
    }

    private async Task RunAsync()
    {
        if (!ResolveBundle(showDialogOnError: true) || lastSelection is null || lastOutDir is null)
        {
            return;
        }

        runButton.Enabled = false;
        resolveButton.Enabled = false;
        stopButton.Enabled = true;
        progressBar.Value = 0;
        try
        {
            var progress = new Progress<RunLaunchSnapshot>(UpdateRunProgress);
            var summary = await launcher.RunAsync(lastSelection, lastOutDir, progress);
            AppendLog($"run finished: {summary.Status}, points={summary.PointsPassed}/{summary.PointsTotal}, frames={summary.FramesTotal}, timeouts={summary.TimeoutCount}, delta_gt1={summary.PacketCounter?.DeltaGt1Count ?? 0}, decode={summary.DecodeFailures ?? 0}");
            stateLabel.Text = $"State: {summary.Status}";
        }
        catch (Exception ex)
        {
            AppendLog($"run failed: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Run failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            stopButton.Enabled = false;
            runButton.Enabled = true;
            resolveButton.Enabled = true;
        }
    }

    private void UpdateRunProgress(RunLaunchSnapshot snapshot)
    {
        stateLabel.Text = $"State: {snapshot.State} - {snapshot.Message}";
        metricsLabel.Text = $"Metrics: frames={snapshot.FramesTotal?.ToString() ?? "-"}, timeout={snapshot.TimeoutCount?.ToString() ?? "-"}, raw_len_bad={snapshot.RawLenBadCount?.ToString() ?? "-"}, delta_gt1={snapshot.DeltaGt1Count?.ToString() ?? "-"}, quality={snapshot.QualityStatus ?? "-"}";
        if (snapshot.PointIndex.HasValue && snapshot.PointsTotal is > 0)
        {
            var completedPoints = snapshot.EventName == "point_completed"
                ? snapshot.PointIndex.Value + 1
                : snapshot.PointIndex.Value;
            var percent = (int)Math.Round(completedPoints * 100.0 / snapshot.PointsTotal.Value);
            progressBar.Value = Math.Max(0, Math.Min(100, percent));
        }
        AppendLog(snapshot.Message);
    }

    private ConfigSelection BuildSelection() =>
        new(
            stationPicker.SelectedPath,
            calibrationPicker.SelectedPath,
            planPicker.SelectedPath,
            smbProfilePicker.SelectedPath,
            oeProfilePicker.SelectedPath,
            laserProfilePicker.SelectedPath);

    private void BrowseOutputRoot()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select output root directory",
            SelectedPath = outputRootBox.Text
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            outputRootBox.Text = dialog.SelectedPath;
        }
    }

    private void InvalidateResolvedBundle()
    {
        lastSelection = null;
        lastBundle = null;
        lastOutDir = null;
        resolveSummaryLabel.Text = "Bundle: not validated";
        outDirLabel.Text = "Out-dir: not generated";
    }

    private void AppendLog(string message)
    {
        eventList.Items.Insert(0, $"{DateTime.Now:HH:mm:ss} {message}");
        while (eventList.Items.Count > 300)
        {
            eventList.Items.RemoveAt(eventList.Items.Count - 1);
        }
    }

    private IEnumerable<ConfigPicker> AllPickers()
    {
        yield return stationPicker;
        yield return calibrationPicker;
        yield return planPicker;
        yield return smbProfilePicker;
        yield return oeProfilePicker;
        yield return laserProfilePicker;
    }

    private static void ValidatePickerJson<T>(ConfigPicker picker, string path, Func<string, T> reader)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            picker.SetStatus("missing", false);
            throw new InvalidOperationException($"{picker.Title} path is required");
        }

        if (!File.Exists(path))
        {
            picker.SetStatus("missing", false);
            throw new InvalidOperationException($"{picker.Title} does not exist: {path}");
        }

        _ = reader(path);
        picker.SetStatus("OK", true);
    }

    private static string BuildSummaryText(RunConfigBundle bundle, string outDir)
    {
        var plan = bundle.ResolvedPlan;
        var defaultSweep = bundle.SmbProfile.DefaultSweep;
        var duration = plan.EstimatedRunDurationMs.HasValue
            ? TimeSpan.FromMilliseconds(plan.EstimatedRunDurationMs.Value).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)
            : "-";
        return string.Join(Environment.NewLine, new[]
        {
            $"run_id: {bundle.Plan.RunId}",
            $"station: {bundle.Station.StationId}",
            $"points: {plan.ResolvedPointCount} ({plan.SourceKind})",
            $"cycle: {plan.CycleMode ?? "-"}",
            $"estimated: {duration}",
            $"SMB: {bundle.SmbProfile.ProfileId}",
            $"RF sweep: {defaultSweep.StartHz:0} -> {defaultSweep.StopHz:0} Hz, step {defaultSweep.StepHz:0} Hz",
            $"RF power: {defaultSweep.PowerDbm:0.###} dBm, dwell {defaultSweep.DwellMs} ms",
            $"OE: {bundle.OeProfile.ProfileId} ({bundle.OeProfile.NormalizedModel})",
            $"RALL: {DescribeCollector(bundle.OeProfile)}",
            $"Laser: {bundle.LaserProfile.ProfileId}, {bundle.LaserProfile.Mode}, {bundle.LaserProfile.PowerMw} mW",
            $"out_dir: {outDir}"
        });
    }

    private static string BuildDetailsText(ConfigSelection selection, RunConfigBundle bundle, string outDir)
    {
        var builder = new StringBuilder();
        builder.AppendLine("[Input files]");
        builder.AppendLine($"station: {selection.StationPath}");
        builder.AppendLine($"calibration: {selection.CalibrationPath}");
        builder.AppendLine($"plan: {selection.PlanPath}");
        builder.AppendLine($"smb_profile: {selection.SmbProfilePath}");
        builder.AppendLine($"oe_profile: {selection.OeProfilePath}");
        builder.AppendLine($"laser_profile: {selection.LaserProfilePath}");
        builder.AppendLine($"out_dir: {outDir}");
        builder.AppendLine();

        builder.AppendLine("[Connections]");
        builder.AppendLine(JsonSerializer.Serialize(bundle.Connections, JsonOptions.Pretty));
        builder.AppendLine();

        builder.AppendLine("[Resolved plan]");
        builder.AppendLine(JsonSerializer.Serialize(bundle.ResolvedPlan, JsonOptions.Pretty));
        builder.AppendLine();

        builder.AppendLine("[Profile ids]");
        builder.AppendLine($"smb: {bundle.SmbProfile.ProfileId}");
        builder.AppendLine($"oe: {bundle.OeProfile.ProfileId}");
        builder.AppendLine($"laser: {bundle.LaserProfile.ProfileId}");
        builder.AppendLine();
        builder.AppendLine("[RALL collector rule]");
        builder.AppendLine(RunConfigLoader.CollectorContractFor(bundle.OeProfile.NormalizedModel));
        return builder.ToString();
    }

    private static string DescribeCollector(OeRunProfile profile)
    {
        if (profile.NormalizedModel == LockinModelNames.Oe1300)
        {
            var collector = profile.GetOe1300Collector();
            return $"{collector.TcpExpectedBytes} bytes, {collector.RallPostWriteDelayMs} ms post-write, {collector.SamplesPerParameter} samples/parameter";
        }

        var oe1022dCollector = profile.GetOe1022dCollector();
        return $"{oe1022dCollector.FrameExactBytes} bytes, {oe1022dCollector.RallPostWriteDelayMs} ms post-write";
    }

    private static string UniqueRunDirectory(string outputRoot, string runId)
    {
        if (string.IsNullOrWhiteSpace(outputRoot))
        {
            throw new InvalidOperationException("output root is required");
        }

        Directory.CreateDirectory(outputRoot);
        var safeRunId = SanitizeId(runId);
        var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        var basePath = Path.Combine(outputRoot, $"{stamp}_{safeRunId}");
        var candidate = basePath;
        var index = 1;
        while (Directory.Exists(candidate))
        {
            candidate = $"{basePath}_{index:00}";
            index++;
        }

        return candidate;
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

    private static void OpenDirectory(string path, bool createIfMissing = false)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var dir = File.Exists(path) ? Path.GetDirectoryName(path) : path;
        if (string.IsNullOrWhiteSpace(dir))
        {
            return;
        }

        if (!Directory.Exists(dir))
        {
            if (!createIfMissing)
            {
                return;
            }

            Directory.CreateDirectory(dir);
        }

        Process.Start(new ProcessStartInfo(dir) { UseShellExecute = true });
    }

    private sealed class ConfigPicker : UserControl
    {
        private readonly ComboBox combo = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown };
        private readonly Label status = new() { Text = "not checked", Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };

        public ConfigPicker(string title)
        {
            Title = title;
            Height = 28;
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 1 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            Controls.Add(table);

            table.Controls.Add(new Label { Text = title, Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            combo.SelectedIndexChanged += (_, _) => SelectionChanged?.Invoke(this, EventArgs.Empty);
            combo.TextChanged += (_, _) => SelectionChanged?.Invoke(this, EventArgs.Empty);
            table.Controls.Add(combo, 1, 0);

            var browse = new Button { Text = "Browse...", Dock = DockStyle.Fill };
            browse.Click += (_, _) => Browse();
            table.Controls.Add(browse, 2, 0);

            var open = new Button { Text = "Open Dir", Dock = DockStyle.Fill };
            open.Click += (_, _) => OpenDirectory(SelectedPath);
            table.Controls.Add(open, 3, 0);
            table.Controls.Add(status, 4, 0);
        }

        public event EventHandler? SelectionChanged;

        public string Title { get; }

        public string SelectedPath
        {
            get => combo.Text;
            private set => combo.Text = value;
        }

        public void LoadPaths(IReadOnlyList<string> paths, string? selectedPath)
        {
            combo.Items.Clear();
            foreach (var path in paths)
            {
                combo.Items.Add(path);
            }
            if (!string.IsNullOrWhiteSpace(selectedPath))
            {
                combo.SelectedItem = selectedPath;
                if (combo.SelectedItem is null)
                {
                    combo.Text = selectedPath;
                }
            }
        }

        public void SetStatus(string text, bool ok)
        {
            status.Text = text;
            status.ForeColor = ok ? Color.DarkGreen : Color.DarkRed;
        }

        private void Browse()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FileName = Path.GetFileName(SelectedPath),
                InitialDirectory = InitialDirectory()
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                SelectedPath = dialog.FileName;
            }
        }

        private string InitialDirectory()
        {
            if (File.Exists(SelectedPath))
            {
                return Path.GetDirectoryName(SelectedPath) ?? Environment.CurrentDirectory;
            }

            if (Directory.Exists(SelectedPath))
            {
                return SelectedPath;
            }

            return Environment.CurrentDirectory;
        }
    }
}
