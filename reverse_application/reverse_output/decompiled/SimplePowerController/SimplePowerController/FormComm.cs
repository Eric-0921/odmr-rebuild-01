using System;
using System.ComponentModel;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;

namespace SimplePowerController;

public class FormComm : Form
{
	private ComboBox[] comboBoxPorts;

	private ComboBox[] comboBoxBaudRates;

	private IContainer components;

	private GroupBox groupBox3;

	private Label label5;

	private ComboBox comboBoxBaudRate3;

	private Label label6;

	private ComboBox comboBoxPort3;

	private GroupBox groupBox2;

	private Label label3;

	private ComboBox comboBoxBaudRate2;

	private Label label4;

	private ComboBox comboBoxPort2;

	private GroupBox groupBox1;

	private Label label2;

	private ComboBox comboBoxBaudRate1;

	private Label label1;

	private ComboBox comboBoxPort1;

	private Button buttonCancel;

	private Button buttonOK;

	public FormComm()
	{
		InitializeComponent();
		comboBoxPorts = new ComboBox[3] { comboBoxPort1, comboBoxPort2, comboBoxPort3 };
		comboBoxBaudRates = new ComboBox[3] { comboBoxBaudRate1, comboBoxBaudRate2, comboBoxBaudRate3 };
		int[] array = new int[9] { 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200 };
		ComboBox[] array2 = comboBoxBaudRates;
		foreach (ComboBox comboBox in array2)
		{
			comboBox.BeginUpdate();
			for (int j = 0; j < array.Length; j++)
			{
				comboBox.Items.Add(array[j]);
			}
			comboBox.EndUpdate();
		}
	}

	private void FormComm_Load(object sender, EventArgs e)
	{
		string[] portNames = SerialPort.GetPortNames();
		LocalSettingAccessor.ReadCommports(out var ports, out var baudRates);
		for (int i = 0; i < 3; i++)
		{
			comboBoxPorts[i].Items.Clear();
			ComboBox.ObjectCollection items = comboBoxPorts[i].Items;
			object[] items2 = portNames;
			items.AddRange(items2);
			if (!string.IsNullOrWhiteSpace(ports[i]) && comboBoxPorts[i].Items.Contains(ports[i]))
			{
				comboBoxPorts[i].SelectedIndex = comboBoxPorts[i].Items.IndexOf(ports[i]);
			}
			comboBoxBaudRates[i].SelectedIndex = comboBoxBaudRates[i].Items.IndexOf(baudRates[i]);
		}
	}

	private void buttonOK_Click(object sender, EventArgs e)
	{
		string[] array = new string[comboBoxPorts.Length];
		int[] array2 = new int[comboBoxBaudRates.Length];
		for (int i = 0; i < comboBoxPorts.Length; i++)
		{
			if (string.IsNullOrWhiteSpace((string)comboBoxPorts[i].SelectedItem))
			{
				array[i] = string.Empty;
			}
			else
			{
				array[i] = comboBoxPorts[i].SelectedItem.ToString();
			}
			array2[i] = (int)comboBoxBaudRates[i].SelectedItem;
		}
		LocalSettingAccessor.SaveCommports(array, array2);
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void buttonCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
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
		this.label5 = new System.Windows.Forms.Label();
		this.comboBoxBaudRate3 = new System.Windows.Forms.ComboBox();
		this.label6 = new System.Windows.Forms.Label();
		this.comboBoxPort3 = new System.Windows.Forms.ComboBox();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.label3 = new System.Windows.Forms.Label();
		this.comboBoxBaudRate2 = new System.Windows.Forms.ComboBox();
		this.label4 = new System.Windows.Forms.Label();
		this.comboBoxPort2 = new System.Windows.Forms.ComboBox();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.label2 = new System.Windows.Forms.Label();
		this.comboBoxBaudRate1 = new System.Windows.Forms.ComboBox();
		this.label1 = new System.Windows.Forms.Label();
		this.comboBoxPort1 = new System.Windows.Forms.ComboBox();
		this.buttonCancel = new System.Windows.Forms.Button();
		this.buttonOK = new System.Windows.Forms.Button();
		this.groupBox3.SuspendLayout();
		this.groupBox2.SuspendLayout();
		this.groupBox1.SuspendLayout();
		base.SuspendLayout();
		this.groupBox3.Controls.Add(this.label5);
		this.groupBox3.Controls.Add(this.comboBoxBaudRate3);
		this.groupBox3.Controls.Add(this.label6);
		this.groupBox3.Controls.Add(this.comboBoxPort3);
		this.groupBox3.Location = new System.Drawing.Point(240, 18);
		this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
		this.groupBox3.Size = new System.Drawing.Size(185, 98);
		this.groupBox3.TabIndex = 12;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "电流源3（Z轴）";
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(16, 65);
		this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(63, 14);
		this.label5.TabIndex = 8;
		this.label5.Text = "波特率：";
		this.comboBoxBaudRate3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBoxBaudRate3.FormattingEnabled = true;
		this.comboBoxBaudRate3.Location = new System.Drawing.Point(93, 61);
		this.comboBoxBaudRate3.Margin = new System.Windows.Forms.Padding(2);
		this.comboBoxBaudRate3.Name = "comboBoxBaudRate3";
		this.comboBoxBaudRate3.Size = new System.Drawing.Size(74, 22);
		this.comboBoxBaudRate3.TabIndex = 7;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(16, 32);
		this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(63, 14);
		this.label6.TabIndex = 6;
		this.label6.Text = "串口号：";
		this.comboBoxPort3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBoxPort3.FormattingEnabled = true;
		this.comboBoxPort3.Location = new System.Drawing.Point(93, 28);
		this.comboBoxPort3.Margin = new System.Windows.Forms.Padding(2);
		this.comboBoxPort3.Name = "comboBoxPort3";
		this.comboBoxPort3.Size = new System.Drawing.Size(74, 22);
		this.comboBoxPort3.TabIndex = 5;
		this.groupBox2.Controls.Add(this.label3);
		this.groupBox2.Controls.Add(this.comboBoxBaudRate2);
		this.groupBox2.Controls.Add(this.label4);
		this.groupBox2.Controls.Add(this.comboBoxPort2);
		this.groupBox2.Location = new System.Drawing.Point(11, 138);
		this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
		this.groupBox2.Size = new System.Drawing.Size(185, 98);
		this.groupBox2.TabIndex = 11;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "电流源2（Y轴）";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(16, 65);
		this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(63, 14);
		this.label3.TabIndex = 8;
		this.label3.Text = "波特率：";
		this.comboBoxBaudRate2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBoxBaudRate2.FormattingEnabled = true;
		this.comboBoxBaudRate2.Location = new System.Drawing.Point(93, 61);
		this.comboBoxBaudRate2.Margin = new System.Windows.Forms.Padding(2);
		this.comboBoxBaudRate2.Name = "comboBoxBaudRate2";
		this.comboBoxBaudRate2.Size = new System.Drawing.Size(74, 22);
		this.comboBoxBaudRate2.TabIndex = 7;
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(16, 32);
		this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(63, 14);
		this.label4.TabIndex = 6;
		this.label4.Text = "串口号：";
		this.comboBoxPort2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBoxPort2.FormattingEnabled = true;
		this.comboBoxPort2.Location = new System.Drawing.Point(93, 28);
		this.comboBoxPort2.Margin = new System.Windows.Forms.Padding(2);
		this.comboBoxPort2.Name = "comboBoxPort2";
		this.comboBoxPort2.Size = new System.Drawing.Size(74, 22);
		this.comboBoxPort2.TabIndex = 5;
		this.groupBox1.Controls.Add(this.label2);
		this.groupBox1.Controls.Add(this.comboBoxBaudRate1);
		this.groupBox1.Controls.Add(this.label1);
		this.groupBox1.Controls.Add(this.comboBoxPort1);
		this.groupBox1.Location = new System.Drawing.Point(11, 18);
		this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
		this.groupBox1.Size = new System.Drawing.Size(185, 98);
		this.groupBox1.TabIndex = 10;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "电流源1（X轴）";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(16, 65);
		this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(63, 14);
		this.label2.TabIndex = 8;
		this.label2.Text = "波特率：";
		this.comboBoxBaudRate1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBoxBaudRate1.FormattingEnabled = true;
		this.comboBoxBaudRate1.Location = new System.Drawing.Point(93, 61);
		this.comboBoxBaudRate1.Margin = new System.Windows.Forms.Padding(2);
		this.comboBoxBaudRate1.Name = "comboBoxBaudRate1";
		this.comboBoxBaudRate1.Size = new System.Drawing.Size(74, 22);
		this.comboBoxBaudRate1.TabIndex = 7;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(16, 32);
		this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(63, 14);
		this.label1.TabIndex = 6;
		this.label1.Text = "串口号：";
		this.comboBoxPort1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBoxPort1.FormattingEnabled = true;
		this.comboBoxPort1.Location = new System.Drawing.Point(93, 28);
		this.comboBoxPort1.Margin = new System.Windows.Forms.Padding(2);
		this.comboBoxPort1.Name = "comboBoxPort1";
		this.comboBoxPort1.Size = new System.Drawing.Size(74, 22);
		this.comboBoxPort1.TabIndex = 5;
		this.buttonCancel.Location = new System.Drawing.Point(328, 255);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(79, 28);
		this.buttonCancel.TabIndex = 19;
		this.buttonCancel.Text = "取消";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.buttonCancel.Click += new System.EventHandler(buttonCancel_Click);
		this.buttonOK.Location = new System.Drawing.Point(234, 255);
		this.buttonOK.Name = "buttonOK";
		this.buttonOK.Size = new System.Drawing.Size(79, 28);
		this.buttonOK.TabIndex = 18;
		this.buttonOK.Text = "确认";
		this.buttonOK.UseVisualStyleBackColor = true;
		this.buttonOK.Click += new System.EventHandler(buttonOK_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 14f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(437, 295);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.buttonOK);
		base.Controls.Add(this.groupBox3);
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.groupBox1);
		this.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FormComm";
		this.Text = "通信设置";
		base.Load += new System.EventHandler(FormComm_Load);
		this.groupBox3.ResumeLayout(false);
		this.groupBox3.PerformLayout();
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		base.ResumeLayout(false);
	}
}
