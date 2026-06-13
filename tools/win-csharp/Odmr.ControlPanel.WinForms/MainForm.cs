using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Odmr.Runtime;

namespace Odmr.ControlPanel.WinForms;

internal sealed class MainForm : Form
{
    private readonly ConfigCatalogService catalog;
    private readonly DraftConfigService drafts = new();
    private readonly RunLaunchService launcher = new();

    private ComboBox stationBox = null!;
    private ComboBox calibrationBox = null!;
    private ComboBox planBox = null!;
    private ComboBox smbProfileBox = null!;
    private ComboBox oeProfileBox = null!;
    private ComboBox laserProfileBox = null!;
    private TextBox outputRootBox = null!;
    private TextBox runIdBox = null!;
    private TextBox operatorBox = null!;
    private ComboBox cycleModeBox = null!;
    private NumericUpDown totalPointsBox = null!;
    private AxisEditor xAxis = null!;
    private AxisEditor yAxis = null!;
    private AxisEditor zAxis = null!;
    private TextBox baselineXBox = null!;
    private TextBox baselineYBox = null!;
    private TextBox baselineZBox = null!;
    private NumericUpDown baselineSettleBox = null!;
    private TextBox voltageBox = null!;
    private TextBox voltageProtectionBox = null!;
    private CheckBox magOutputBox = null!;
    private TextBox smbStartBox = null!;
    private TextBox smbStopBox = null!;
    private TextBox smbStepBox = null!;
    private NumericUpDown smbDwellBox = null!;
    private TextBox smbPowerBox = null!;
    private CheckBox smbRfOutputBox = null!;
    private NumericUpDown oeTimeConstantBox = null!;
    private NumericUpDown oeFilterSlopeBox = null!;
    private Label resolveSummaryLabel = null!;
    private Label outDirLabel = null!;
    private Label stateLabel = null!;
    private Label metricsLabel = null!;
    private ProgressBar progressBar = null!;
    private ListBox eventList = null!;
    private TextBox diffBox = null!;
    private Button resolveButton = null!;
    private Button runButton = null!;
    private Button stopButton = null!;

    private DraftResult? lastDraft;

    public MainForm()
    {
        Text = "ODMR Control Panel";
        Width = 1180;
        Height = 820;
        MinimumSize = new Size(980, 700);
        catalog = new ConfigCatalogService();
        BuildUi();
        LoadCatalogs();
        LoadDefaultsFromSelectedFiles();
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
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        Controls.Add(root);

        root.Controls.Add(BuildRunSetupPanel(), 0, 0);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildPlanTab());
        tabs.TabPages.Add(BuildProfileTab());
        tabs.TabPages.Add(BuildAdvancedTab());
        root.Controls.Add(tabs, 0, 1);
        root.Controls.Add(BuildRunStatusPanel(), 0, 2);
    }

    private Control BuildRunSetupPanel()
    {
        var group = new GroupBox { Text = "Run Setup", Dock = DockStyle.Fill };
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 4, Padding = new Padding(8) };
        for (var i = 0; i < 6; i++)
        {
            table.ColumnStyles.Add(new ColumnStyle(i % 2 == 0 ? SizeType.Absolute : SizeType.Percent, i % 2 == 0 ? 95 : 33));
        }
        group.Controls.Add(table);

        stationBox = AddCombo(table, "Station", 0, 0);
        calibrationBox = AddCombo(table, "Calibration", 2, 0);
        planBox = AddCombo(table, "Plan source", 4, 0);
        smbProfileBox = AddCombo(table, "SMB profile", 0, 1);
        oeProfileBox = AddCombo(table, "OE profile", 2, 1);
        laserProfileBox = AddCombo(table, "Laser profile", 4, 1);

        table.Controls.Add(new Label { Text = "Output root", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 2);
        outputRootBox = new TextBox { Dock = DockStyle.Fill };
        table.Controls.Add(outputRootBox, 1, 2);
        table.SetColumnSpan(outputRootBox, 3);
        var browse = new Button { Text = "Browse...", Dock = DockStyle.Fill };
        browse.Click += (_, _) => BrowseOutputRoot();
        table.Controls.Add(browse, 4, 2);

        resolveButton = new Button { Text = "Resolve / Save Draft", Dock = DockStyle.Fill };
        resolveButton.Click += (_, _) => ResolveDraft();
        table.Controls.Add(resolveButton, 5, 2);

        resolveSummaryLabel = new Label { Text = "Resolve summary: not resolved", Dock = DockStyle.Fill, AutoEllipsis = true };
        table.Controls.Add(resolveSummaryLabel, 0, 3);
        table.SetColumnSpan(resolveSummaryLabel, 4);
        outDirLabel = new Label { Text = "Out-dir: not generated", Dock = DockStyle.Fill, AutoEllipsis = true };
        table.Controls.Add(outDirLabel, 4, 3);
        table.SetColumnSpan(outDirLabel, 2);

        foreach (var combo in new[] { planBox, smbProfileBox, oeProfileBox })
        {
            combo.SelectedIndexChanged += (_, _) => LoadDefaultsFromSelectedFiles();
        }

        return group;
    }

    private TabPage BuildPlanTab()
    {
        var page = new TabPage("Plan Builder");
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(8) };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 135));
        page.Controls.Add(table);

        var axisGroup = new GroupBox { Text = "Cartesian Field Grid (nT)", Dock = DockStyle.Fill };
        var axisPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(8) };
        axisGroup.Controls.Add(axisPanel);
        xAxis = new AxisEditor("X");
        yAxis = new AxisEditor("Y");
        zAxis = new AxisEditor("Z");
        axisPanel.Controls.Add(xAxis);
        axisPanel.Controls.Add(yAxis);
        axisPanel.Controls.Add(zAxis);
        table.Controls.Add(axisGroup, 0, 0);

        var planGroup = new GroupBox { Text = "Plan Identity", Dock = DockStyle.Fill };
        var planTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, Padding = new Padding(8) };
        planTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        planTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        planGroup.Controls.Add(planTable);
        runIdBox = AddText(planTable, "Run ID", 0, "ui_grid_run");
        operatorBox = AddText(planTable, "Operator", 1, "local");
        cycleModeBox = AddComboRaw(planTable, "Cycle mode", 2, new[] { "raster", "bounce_1d_x" });
        totalPointsBox = AddNumeric(planTable, "Total points", 3, 1, 1_000_000, 1);
        planTable.Controls.Add(new Label { Text = "RALL locked", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 4);
        planTable.Controls.Add(new Label { Text = "12288 bytes, 30ms post-write", Anchor = AnchorStyles.Left, AutoSize = true }, 1, 4);
        table.Controls.Add(planGroup, 1, 0);

        var baselineGroup = new GroupBox { Text = "Maynuo Baseline / Output", Dock = DockStyle.Fill };
        var baselineTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 8, RowCount = 2, Padding = new Padding(8) };
        baselineGroup.Controls.Add(baselineTable);
        baselineXBox = AddSmallText(baselineTable, "Base X A", 0, "0.0");
        baselineYBox = AddSmallText(baselineTable, "Base Y A", 2, "0.0");
        baselineZBox = AddSmallText(baselineTable, "Base Z A", 4, "0.0");
        baselineSettleBox = AddNumericInline(baselineTable, "Settle ms", 6, 1000);
        voltageBox = AddSmallText(baselineTable, "Volt", 0, "75.0", row: 1);
        voltageProtectionBox = AddSmallText(baselineTable, "V prot", 2, "75.0", row: 1);
        magOutputBox = new CheckBox { Text = "Output enabled", Checked = true, Anchor = AnchorStyles.Left, AutoSize = true };
        baselineTable.Controls.Add(magOutputBox, 4, 1);
        table.Controls.Add(baselineGroup, 0, 1);
        table.SetColumnSpan(baselineGroup, 2);

        return page;
    }

    private TabPage BuildProfileTab()
    {
        var page = new TabPage("Profiles");
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(8) };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        page.Controls.Add(table);

        var smbGroup = new GroupBox { Text = "SMB100A Sweep Overrides", Dock = DockStyle.Fill };
        var smbTable = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 7, Padding = new Padding(8), AutoSize = true };
        smbTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        smbTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        smbGroup.Controls.Add(smbTable);
        smbStartBox = AddText(smbTable, "Start Hz", 0, "2830000000");
        smbStopBox = AddText(smbTable, "Stop Hz", 1, "2890000000");
        smbStepBox = AddText(smbTable, "Step Hz", 2, "500000");
        smbDwellBox = AddNumeric(smbTable, "Dwell ms", 3, 1, 1_000_000, 300);
        smbPowerBox = AddText(smbTable, "Power dBm", 4, "-10");
        smbRfOutputBox = new CheckBox { Text = "RF output enabled", Checked = true, Anchor = AnchorStyles.Left, AutoSize = true };
        smbTable.Controls.Add(new Label { Text = "RF output", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 5);
        smbTable.Controls.Add(smbRfOutputBox, 1, 5);
        table.Controls.Add(smbGroup, 0, 0);

        var oeGroup = new GroupBox { Text = "OE1022D Common Fields", Dock = DockStyle.Fill };
        var oeTable = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 7, Padding = new Padding(8), AutoSize = true };
        oeTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        oeTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        oeGroup.Controls.Add(oeTable);
        oeTimeConstantBox = AddNumeric(oeTable, "Time constant index", 0, 0, 255, 9);
        oeFilterSlopeBox = AddNumeric(oeTable, "Filter slope", 1, 0, 255, 1);
        oeTable.Controls.Add(new Label { Text = "RALL frame bytes", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 2);
        oeTable.Controls.Add(new Label { Text = "12288 (locked)", Anchor = AnchorStyles.Left, AutoSize = true }, 1, 2);
        oeTable.Controls.Add(new Label { Text = "RALL post-write", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 3);
        oeTable.Controls.Add(new Label { Text = "30 ms (locked)", Anchor = AnchorStyles.Left, AutoSize = true }, 1, 3);
        oeTable.Controls.Add(new Label { Text = "Hot path", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 4);
        oeTable.Controls.Add(new Label { Text = RuntimeContracts.FrozenRallHotPath, Anchor = AnchorStyles.Left, AutoSize = true }, 1, 4);
        table.Controls.Add(oeGroup, 1, 0);

        return page;
    }

    private TabPage BuildAdvancedTab()
    {
        var page = new TabPage("Advanced JSON / Diff");
        diffBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            Font = new Font(FontFamily.GenericMonospace, 9),
            ReadOnly = true
        };
        page.Controls.Add(diffBox);
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
        LoadCombo(stationBox, catalog.Stations(), "lab_a.json");
        LoadCombo(calibrationBox, catalog.Calibrations(), "main.json");
        LoadCombo(planBox, catalog.Plans(), "x_axis_1d_bounce_15min.json");
        LoadCombo(smbProfileBox, catalog.Profiles("smb100a"), "smb100a_run_monitor_2830_2890_-10dbm.json");
        LoadCombo(oeProfileBox, catalog.Profiles("oe1022d"), "oe1022d_run_ch_b_observed.json");
        LoadCombo(laserProfileBox, catalog.Profiles("cni_laser"), "cni_laser_run_off_background.json");
        outputRootBox.Text = catalog.DefaultOutputRoot();
    }

    private static void LoadCombo(ComboBox box, IReadOnlyList<string> paths, string preferredName)
    {
        box.Items.Clear();
        foreach (var path in paths)
        {
            box.Items.Add(path);
        }
        var preferred = paths.FirstOrDefault(path => Path.GetFileName(path).Equals(preferredName, StringComparison.OrdinalIgnoreCase));
        box.SelectedItem = preferred ?? paths.FirstOrDefault();
    }

    private void LoadDefaultsFromSelectedFiles()
    {
        try
        {
            if (File.Exists(planBox.Text))
            {
                var plan = RunConfigLoader.ReadJson<AcquisitionRunPlan>(planBox.Text);
                runIdBox.Text = $"{plan.RunId}_ui";
                operatorBox.Text = plan.Operator;
                baselineXBox.Text = plan.MagBaselinePolicy.BaselineCurrentA.ElementAtOrDefault(0).ToString(CultureInfo.InvariantCulture);
                baselineYBox.Text = plan.MagBaselinePolicy.BaselineCurrentA.ElementAtOrDefault(1).ToString(CultureInfo.InvariantCulture);
                baselineZBox.Text = plan.MagBaselinePolicy.BaselineCurrentA.ElementAtOrDefault(2).ToString(CultureInfo.InvariantCulture);
                baselineSettleBox.Value = ClampDecimal(plan.MagBaselinePolicy.SettleMs, baselineSettleBox.Minimum, baselineSettleBox.Maximum);
                voltageBox.Text = (plan.MagBaselinePolicy.VoltageV ?? 75.0).ToString(CultureInfo.InvariantCulture);
                voltageProtectionBox.Text = (plan.MagBaselinePolicy.VoltageProtectionV ?? 75.0).ToString(CultureInfo.InvariantCulture);
                magOutputBox.Checked = plan.MagBaselinePolicy.OutputEnabled;
                if (plan.PointSource is not null)
                {
                    xAxis.SetExplicit(plan.PointSource.AxesNt.X);
                    yAxis.SetExplicit(plan.PointSource.AxesNt.Y);
                    zAxis.SetExplicit(plan.PointSource.AxesNt.Z);
                    cycleModeBox.SelectedItem = plan.PointSource.CycleMode;
                    totalPointsBox.Value = ClampDecimal(plan.PointSource.StopCondition.TotalPoints, totalPointsBox.Minimum, totalPointsBox.Maximum);
                }
            }

            if (File.Exists(smbProfileBox.Text))
            {
                var smb = RunConfigLoader.ReadJson<Smb100aRunProfile>(smbProfileBox.Text);
                smbStartBox.Text = smb.DefaultSweep.StartHz.ToString(CultureInfo.InvariantCulture);
                smbStopBox.Text = smb.DefaultSweep.StopHz.ToString(CultureInfo.InvariantCulture);
                smbStepBox.Text = smb.DefaultSweep.StepHz.ToString(CultureInfo.InvariantCulture);
                smbDwellBox.Value = ClampDecimal(smb.DefaultSweep.DwellMs, smbDwellBox.Minimum, smbDwellBox.Maximum);
                smbPowerBox.Text = smb.DefaultSweep.PowerDbm.ToString(CultureInfo.InvariantCulture);
                smbRfOutputBox.Checked = smb.DefaultSweep.RfOutputEnabled;
            }

            if (File.Exists(oeProfileBox.Text))
            {
                var oe = RunConfigLoader.ReadJson<Oe1022dRunProfile>(oeProfileBox.Text);
                oeTimeConstantBox.Value = ClampDecimal(oe.Fixed.TimeConstantIndex, oeTimeConstantBox.Minimum, oeTimeConstantBox.Maximum);
                oeFilterSlopeBox.Value = ClampDecimal(oe.Fixed.FilterSlope, oeFilterSlopeBox.Minimum, oeFilterSlopeBox.Maximum);
            }
        }
        catch (Exception ex)
        {
            AppendLog($"load defaults failed: {ex.Message}");
        }
    }

    private void ResolveDraft()
    {
        try
        {
            lastDraft = drafts.SaveDrafts(BuildSelection(), BuildPlanDraft(), BuildProfileDraft(), catalog.RepoRoot, outputRootBox.Text);
            var r = lastDraft.Resolution;
            var duration = lastDraft.EstimatedRunDurationMs.HasValue
                ? TimeSpan.FromMilliseconds(lastDraft.EstimatedRunDurationMs.Value).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)
                : "-";
            resolveSummaryLabel.Text = $"Resolved: {r.ResolvedPointCount} points, est={duration}, source={r.SourceKind}, first={r.FirstPoint?.PointId}, last={r.LastPoint?.PointId}, SMB={r.SmbProfileId}, OE={r.OeProfileId}";
            outDirLabel.Text = $"Out-dir: {lastDraft.OutDir}";
            diffBox.Text = lastDraft.Paths.DiffText;
            AppendLog("resolved and saved generated draft configs");
        }
        catch (Exception ex)
        {
            lastDraft = null;
            AppendLog($"resolve failed: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Resolve failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task RunAsync()
    {
        ResolveDraft();
        if (lastDraft is null)
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
            var summary = await launcher.RunAsync(BuildSelection(), lastDraft.Paths, lastDraft.OutDir, progress);
            AppendLog($"run finished: {summary.Status}, points={summary.PointsPassed}/{summary.PointsTotal}, frames={summary.FramesTotal}, timeouts={summary.TimeoutCount}, delta_gt1={summary.PacketCounter.DeltaGt1Count}");
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
        new(stationBox.Text, calibrationBox.Text, planBox.Text, smbProfileBox.Text, oeProfileBox.Text, laserProfileBox.Text);

    private PlanDraft BuildPlanDraft() =>
        new(
            runIdBox.Text,
            operatorBox.Text,
            xAxis.ToDraft(),
            yAxis.ToDraft(),
            zAxis.ToDraft(),
            cycleModeBox.Text,
            (int)totalPointsBox.Value,
            ParseDouble(baselineXBox.Text, "baseline X"),
            ParseDouble(baselineYBox.Text, "baseline Y"),
            ParseDouble(baselineZBox.Text, "baseline Z"),
            (int)baselineSettleBox.Value,
            ParseDouble(voltageBox.Text, "voltage"),
            ParseDouble(voltageProtectionBox.Text, "voltage protection"),
            magOutputBox.Checked);

    private ProfileDraft BuildProfileDraft() =>
        new(
            ParseDouble(smbStartBox.Text, "SMB start"),
            ParseDouble(smbStopBox.Text, "SMB stop"),
            ParseDouble(smbStepBox.Text, "SMB step"),
            (int)smbDwellBox.Value,
            ParseDouble(smbPowerBox.Text, "SMB power"),
            smbRfOutputBox.Checked,
            (int)oeTimeConstantBox.Value,
            (int)oeFilterSlopeBox.Value);

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

    private void AppendLog(string message)
    {
        eventList.Items.Insert(0, $"{DateTime.Now:HH:mm:ss} {message}");
        while (eventList.Items.Count > 300)
        {
            eventList.Items.RemoveAt(eventList.Items.Count - 1);
        }
    }

    private static ComboBox AddCombo(TableLayoutPanel table, string label, int column, int row)
    {
        table.Controls.Add(new Label { Text = label, Anchor = AnchorStyles.Left, AutoSize = true }, column, row);
        var box = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown };
        table.Controls.Add(box, column + 1, row);
        return box;
    }

    private static ComboBox AddComboRaw(TableLayoutPanel table, string label, int row, string[] values)
    {
        table.Controls.Add(new Label { Text = label, Anchor = AnchorStyles.Left, AutoSize = true }, 0, row);
        var box = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        box.Items.AddRange(values.Cast<object>().ToArray());
        box.SelectedIndex = 0;
        table.Controls.Add(box, 1, row);
        return box;
    }

    private static TextBox AddText(TableLayoutPanel table, string label, int row, string value)
    {
        table.Controls.Add(new Label { Text = label, Anchor = AnchorStyles.Left, AutoSize = true }, 0, row);
        var box = new TextBox { Dock = DockStyle.Fill, Text = value };
        table.Controls.Add(box, 1, row);
        return box;
    }

    private static TextBox AddSmallText(TableLayoutPanel table, string label, int column, string value, int row = 0)
    {
        table.Controls.Add(new Label { Text = label, Anchor = AnchorStyles.Left, AutoSize = true }, column, row);
        var box = new TextBox { Dock = DockStyle.Fill, Text = value };
        table.Controls.Add(box, column + 1, row);
        return box;
    }

    private static NumericUpDown AddNumeric(TableLayoutPanel table, string label, int row, decimal min, decimal max, decimal value)
    {
        table.Controls.Add(new Label { Text = label, Anchor = AnchorStyles.Left, AutoSize = true }, 0, row);
        var box = new NumericUpDown { Dock = DockStyle.Fill, Minimum = min, Maximum = max, Value = value };
        table.Controls.Add(box, 1, row);
        return box;
    }

    private static NumericUpDown AddNumericInline(TableLayoutPanel table, string label, int column, decimal value)
    {
        table.Controls.Add(new Label { Text = label, Anchor = AnchorStyles.Left, AutoSize = true }, column, 0);
        var box = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0, Maximum = 1_000_000, Value = value };
        table.Controls.Add(box, column + 1, 0);
        return box;
    }

    private static double ParseDouble(string value, string name) =>
        double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : throw new InvalidOperationException($"{name} must be a number: {value}");

    private static decimal ClampDecimal(decimal value, decimal min, decimal max) => Math.Max(min, Math.Min(max, value));

    private sealed class AxisEditor : UserControl
    {
        private readonly CheckBox explicitCheck = new() { Text = "Explicit", AutoSize = true, Anchor = AnchorStyles.Left };
        private readonly TextBox explicitBox = new() { Dock = DockStyle.Fill };
        private readonly TextBox startBox = new() { Dock = DockStyle.Fill };
        private readonly TextBox stopBox = new() { Dock = DockStyle.Fill };
        private readonly TextBox stepBox = new() { Dock = DockStyle.Fill };

        public AxisEditor(string axis)
        {
            Dock = DockStyle.Top;
            Height = 58;
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 9, RowCount = 2 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            Controls.Add(table);
            table.Controls.Add(new Label { Text = axis, Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            table.Controls.Add(explicitCheck, 1, 0);
            table.Controls.Add(new Label { Text = "List", Anchor = AnchorStyles.Left, AutoSize = true }, 3, 0);
            table.Controls.Add(explicitBox, 4, 0);
            table.SetColumnSpan(explicitBox, 5);
            table.Controls.Add(new Label { Text = "Start", Anchor = AnchorStyles.Left, AutoSize = true }, 1, 1);
            table.Controls.Add(startBox, 2, 1);
            table.Controls.Add(new Label { Text = "Stop", Anchor = AnchorStyles.Left, AutoSize = true }, 3, 1);
            table.Controls.Add(stopBox, 4, 1);
            table.Controls.Add(new Label { Text = "Step", Anchor = AnchorStyles.Left, AutoSize = true }, 5, 1);
            table.Controls.Add(stepBox, 6, 1);
            table.Controls.Add(new Label { Text = "nT", Anchor = AnchorStyles.Left, AutoSize = true }, 7, 1);
            startBox.Text = "0";
            stopBox.Text = "0";
            stepBox.Text = "10";
            explicitBox.Text = "0";
        }

        public void SetExplicit(IReadOnlyList<double> values)
        {
            explicitCheck.Checked = true;
            explicitBox.Text = string.Join(", ", values.Select(value => value.ToString(CultureInfo.InvariantCulture)));
            if (values.Count > 0)
            {
                startBox.Text = values.First().ToString(CultureInfo.InvariantCulture);
                stopBox.Text = values.Last().ToString(CultureInfo.InvariantCulture);
            }
            stepBox.Text = values.Count > 1 ? (values[1] - values[0]).ToString(CultureInfo.InvariantCulture) : "10";
        }

        public AxisDraft ToDraft() =>
            new(
                explicitCheck.Checked,
                explicitBox.Text,
                ParseDouble(startBox.Text, "axis start"),
                ParseDouble(stopBox.Text, "axis stop"),
                ParseDouble(stepBox.Text, "axis step"));
    }
}
