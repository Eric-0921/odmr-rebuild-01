using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SimplePowerController;

public class FormSaveDirSet : Form
{
	public string SavePath;

	public double SamplingInterval;

	private IContainer components;

	private GroupBox groupBox3;

	private TextBox textBoxSaveDir;

	private Button buttonSelSaveDir;

	private Button buttonCancel;

	private Button buttonOK;

	private GroupBox groupBox1;

	private TextBox textBoxFileName;

	private GroupBox groupBox2;

	private Label label1;

	private Label label3;

	private Label label2;

	private NumericUpDown numericUpDown1;

	public FormSaveDirSet()
	{
		InitializeComponent();
	}

	private void FormSaveDirSet_Load(object sender, EventArgs e)
	{
		textBoxSaveDir.Text = LocalSettingAccessor.DataSaveDir;
		textBoxFileName.Text = string.Format("{0}.txt", DateTime.Now.ToString("yyyyMMddHHmmss"));
	}

	private void buttonOK_Click(object sender, EventArgs e)
	{
		string text = textBoxSaveDir.Text;
		if (!Directory.Exists(text))
		{
			MessageBox.Show("保存路径不存在！");
			return;
		}
		SavePath = $"{text}\\{textBoxFileName.Text}";
		if (File.Exists(SavePath))
		{
			MessageBox.Show("已存在同名的文件！");
			return;
		}
		try
		{
			using StreamWriter streamWriter = File.AppendText(SavePath);
			streamWriter.WriteLine("时间,x零场偏置,x复现电流,x复现磁场磁场值,y零场偏置,y复现电流,y复现磁场磁场值,z零场偏置,z复现电流,z复现磁场磁场值");
			streamWriter.Flush();
		}
		catch (Exception ex)
		{
			MessageBox.Show("请输入合法的文件名！\n" + ex.Message);
			return;
		}
		SamplingInterval = (double)numericUpDown1.Value;
		LocalSettingAccessor.DataSaveDir = textBoxSaveDir.Text;
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void buttonCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void buttonSelSaveDir_Click(object sender, EventArgs e)
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		if (Directory.Exists(textBoxSaveDir.Text))
		{
			folderBrowserDialog.SelectedPath = textBoxSaveDir.Text;
		}
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			textBoxSaveDir.Text = folderBrowserDialog.SelectedPath;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.textBoxSaveDir = new System.Windows.Forms.TextBox();
		this.buttonSelSaveDir = new System.Windows.Forms.Button();
		this.buttonCancel = new System.Windows.Forms.Button();
		this.buttonOK = new System.Windows.Forms.Button();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.label1 = new System.Windows.Forms.Label();
		this.textBoxFileName = new System.Windows.Forms.TextBox();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.label3 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
		this.groupBox3.SuspendLayout();
		this.groupBox1.SuspendLayout();
		this.groupBox2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown1).BeginInit();
		base.SuspendLayout();
		this.groupBox3.Controls.Add(this.textBoxSaveDir);
		this.groupBox3.Controls.Add(this.buttonSelSaveDir);
		this.groupBox3.Location = new System.Drawing.Point(26, 36);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Size = new System.Drawing.Size(563, 105);
		this.groupBox3.TabIndex = 61;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "数据保存路径";
		this.textBoxSaveDir.Location = new System.Drawing.Point(26, 40);
		this.textBoxSaveDir.Name = "textBoxSaveDir";
		this.textBoxSaveDir.ReadOnly = true;
		this.textBoxSaveDir.Size = new System.Drawing.Size(458, 23);
		this.textBoxSaveDir.TabIndex = 57;
		this.buttonSelSaveDir.Location = new System.Drawing.Point(492, 34);
		this.buttonSelSaveDir.Name = "buttonSelSaveDir";
		this.buttonSelSaveDir.Size = new System.Drawing.Size(44, 32);
		this.buttonSelSaveDir.TabIndex = 58;
		this.buttonSelSaveDir.Text = "...";
		this.buttonSelSaveDir.UseVisualStyleBackColor = true;
		this.buttonSelSaveDir.Click += new System.EventHandler(buttonSelSaveDir_Click);
		this.buttonCancel.Location = new System.Drawing.Point(477, 310);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(94, 28);
		this.buttonCancel.TabIndex = 63;
		this.buttonCancel.Text = "取消";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.buttonCancel.Click += new System.EventHandler(buttonCancel_Click);
		this.buttonOK.Location = new System.Drawing.Point(329, 310);
		this.buttonOK.Name = "buttonOK";
		this.buttonOK.Size = new System.Drawing.Size(113, 28);
		this.buttonOK.TabIndex = 62;
		this.buttonOK.Text = "开始保存";
		this.buttonOK.UseVisualStyleBackColor = true;
		this.buttonOK.Click += new System.EventHandler(buttonOK_Click);
		this.groupBox1.Controls.Add(this.label1);
		this.groupBox1.Controls.Add(this.textBoxFileName);
		this.groupBox1.Location = new System.Drawing.Point(26, 182);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(303, 84);
		this.groupBox1.TabIndex = 64;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "设置保存文件名";
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(16, 37);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(91, 14);
		this.label1.TabIndex = 1;
		this.label1.Text = "保存文件名：";
		this.textBoxFileName.Location = new System.Drawing.Point(112, 34);
		this.textBoxFileName.Name = "textBoxFileName";
		this.textBoxFileName.Size = new System.Drawing.Size(168, 23);
		this.textBoxFileName.TabIndex = 0;
		this.groupBox2.Controls.Add(this.label3);
		this.groupBox2.Controls.Add(this.label2);
		this.groupBox2.Controls.Add(this.numericUpDown1);
		this.groupBox2.Location = new System.Drawing.Point(346, 182);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(242, 83);
		this.groupBox2.TabIndex = 65;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "设置采样频率";
		this.groupBox2.Visible = false;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(28, 39);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(77, 14);
		this.label3.TabIndex = 2;
		this.label3.Text = "采样频率：";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(196, 39);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(14, 14);
		this.label2.TabIndex = 1;
		this.label2.Text = "s";
		this.numericUpDown1.DecimalPlaces = 1;
		this.numericUpDown1.Increment = new decimal(new int[4] { 1, 0, 0, 65536 });
		this.numericUpDown1.Location = new System.Drawing.Point(111, 35);
		this.numericUpDown1.Maximum = new decimal(new int[4] { 60, 0, 0, 0 });
		this.numericUpDown1.Minimum = new decimal(new int[4] { 2, 0, 0, 65536 });
		this.numericUpDown1.Name = "numericUpDown1";
		this.numericUpDown1.Size = new System.Drawing.Size(79, 23);
		this.numericUpDown1.TabIndex = 0;
		this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown1.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 14f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(614, 374);
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.buttonOK);
		base.Controls.Add(this.groupBox3);
		this.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Margin = new System.Windows.Forms.Padding(4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FormSaveDirSet";
		this.Text = "记录数据";
		base.Load += new System.EventHandler(FormSaveDirSet_Load);
		this.groupBox3.ResumeLayout(false);
		this.groupBox3.PerformLayout();
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown1).EndInit();
		base.ResumeLayout(false);
	}
}
