using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CControls;
using SimplePowerController.Properties;

namespace SimplePowerController;

public class FormMain : Form
{
	private readonly double POWER_MAX_CURR = 5000.0;

	private SerialPort[] recurrencePorts = new SerialPort[3];

	private double coilConstantX;

	private double coilConstantY;

	private double coilConstantZ;

	private bool save_Flag;

	private string savePath;

	private StreamWriter dataWriter;

	private bool saveLedRed;

	private IContainer components;

	private ToolStrip toolStrip1;

	private ToolStripButton toolStripButton1;

	private ToolStripButton toolStripButton2;

	private ToolStripButton toolStripButton3;

	private ToolStripButton toolStripButton4;

	private ToolStripButton toolStripButton5;

	private Panel panel1;

	private Label label8;

	private CToggleButton cToggleButtonOutputX;

	private Label label10;

	private TextBox textBoxRecurMagX;

	private Label label24;

	private Label label29;

	private Label label62;

	private Label label59;

	private TextBox textBoxTotalCurrX;

	private Label label89;

	private Label label57;

	private Label label58;

	private Label label1;

	private Label label3;

	private Label label2;

	private Label label4;

	private Label label5;

	private TextBox textBoxZeroCurrX;

	private Label label6;

	private CToggleButton cToggleButtonLockZeroX;

	private Label label7;

	private Label label9;

	private TextBox textBoxRecurCurrX;

	private Panel panel2;

	private Label label11;

	private Label label12;

	private TextBox textBoxRecurCurrY;

	private Label label13;

	private CToggleButton cToggleButtonLockZeroY;

	private Label label14;

	private Label label15;

	private TextBox textBoxZeroCurrY;

	private Label label19;

	private CToggleButton cToggleButtonOutputY;

	private TextBox textBoxRecurMagY;

	private Label label21;

	private Label label22;

	private Label label23;

	private Label label25;

	private TextBox textBoxTotalCurrY;

	private Label label26;

	private Panel panel3;

	private Label label30;

	private Label label31;

	private TextBox textBoxRecurCurrZ;

	private Label label32;

	private CToggleButton cToggleButtonLockZeroZ;

	private Label label33;

	private Label label34;

	private TextBox textBoxZeroCurrZ;

	private Label label38;

	private CToggleButton cToggleButtonOutputZ;

	private TextBox textBoxRecurMagZ;

	private Label label40;

	private Label label41;

	private Label label42;

	private Label label43;

	private TextBox textBoxTotalCurrZ;

	private Label label44;

	private CRegulator6x1 recurSetMagX;

	private CRegulator4x2 recurSetCurrX;

	private CRegulator4x2 zeroSetCurrX;

	private CRegulator6x1 recurSetMagY;

	private CRegulator4x2 recurSetCurrY;

	private CRegulator4x2 zeroSetCurrY;

	private Label label16;

	private Label label17;

	private Label label18;

	private Label label20;

	private Label label27;

	private Label label28;

	private CRegulator6x1 recurSetMagZ;

	private CRegulator4x2 recurSetCurrZ;

	private CRegulator4x2 zeroSetCurrZ;

	private Label label35;

	private Label label36;

	private Label label37;

	private Label label39;

	private Label label45;

	private Label label46;

	private System.Windows.Forms.Timer timer1;

	private System.Windows.Forms.Timer timer2;

	private ToolStripButton toolStripButton6;

	public FormMain()
	{
		InitializeComponent();
	}

	private void FormMain_Load(object sender, EventArgs e)
	{
		Version version = Assembly.GetExecutingAssembly().GetName().Version;
		Text += $"V{version.Major}.{version.Minor}.{version.Build}";
		LocalSettingAccessor.ReadZeroOffset(out var zeroOffsetX, out var zeroOffsetY, out var zeroOffsetZ);
		zeroSetCurrX.Value = zeroOffsetX;
		zeroSetCurrY.Value = zeroOffsetY;
		zeroSetCurrZ.Value = zeroOffsetZ;
		LocalSettingAccessor.ReadCoilConstant(out var num, out var num2, out var num3);
		coilConstantX = num;
		coilConstantY = num2;
		coilConstantZ = num3;
	}

	private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (!toolStripButton1.Text.Equals("连接设备"))
		{
			MessageBox.Show("退出程序前，请先断开设备连接！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			e.Cancel = true;
		}
	}

	private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
	{
		LocalSettingAccessor.SaveZeroOffset(zeroSetCurrX.Value, zeroSetCurrY.Value, zeroSetCurrZ.Value);
	}

	private void toolStripButton1_Click(object sender, EventArgs e)
	{
		if (toolStripButton1.Text.Equals("连接设备"))
		{
			if (checkHardware())
			{
				SerialPort[] array = recurrencePorts;
				foreach (SerialPort obj in array)
				{
					obj.Write("SYST:REM\n");
					obj.Write("VOLT 75\n");
					obj.Write("CURR 0\n");
					obj.Write("OUTP 0\n");
				}
				recurrencePorts[0].DataReceived -= RecurrenceX_DataReceived;
				recurrencePorts[0].DataReceived += RecurrenceX_DataReceived;
				recurrencePorts[1].DataReceived -= RecurrenceY_DataReceived;
				recurrencePorts[1].DataReceived += RecurrenceY_DataReceived;
				recurrencePorts[2].DataReceived -= RecurrenceZ_DataReceived;
				recurrencePorts[2].DataReceived += RecurrenceZ_DataReceived;
				toolStripButton1.Image = Resources.连接;
				toolStripButton1.Text = "断开连接";
				cToggleButtonLockZeroX.Enabled = false;
				cToggleButtonLockZeroY.Enabled = false;
				cToggleButtonLockZeroZ.Enabled = false;
				cToggleButtonOutputX.Enabled = true;
				cToggleButtonOutputY.Enabled = true;
				cToggleButtonOutputZ.Enabled = true;
				timer1.Interval = 300;
				timer1.Start();
			}
		}
		else if ((!cToggleButtonOutputX.Checked && !cToggleButtonOutputY.Checked && !cToggleButtonOutputZ.Checked) || MessageBox.Show("断开连接将关闭电源输出，是否继续？", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.No)
		{
			cToggleButtonLockZeroX.Checked = false;
			cToggleButtonLockZeroY.Checked = false;
			cToggleButtonLockZeroZ.Checked = false;
			cToggleButtonOutputX.Checked = false;
			cToggleButtonOutputY.Checked = false;
			cToggleButtonOutputZ.Checked = false;
			timer1.Stop();
			SerialPort[] array = recurrencePorts;
			foreach (SerialPort obj2 in array)
			{
				obj2.Write("SYST:LOC\n");
				Thread.Sleep(20);
				obj2.Close();
			}
			recurrencePorts[0].DataReceived -= RecurrenceX_DataReceived;
			recurrencePorts[1].DataReceived -= RecurrenceY_DataReceived;
			recurrencePorts[2].DataReceived -= RecurrenceZ_DataReceived;
			toolStripButton1.Image = Resources.断开连接;
			toolStripButton1.Text = "连接设备";
			cToggleButtonOutputX.Enabled = false;
			cToggleButtonOutputY.Enabled = false;
			cToggleButtonOutputZ.Enabled = false;
		}
	}

	private void toolStripButton2_Click(object sender, EventArgs e)
	{
		new FormComm().ShowDialog(this);
	}

	private void toolStripButton3_Click(object sender, EventArgs e)
	{
		if (new FormParamSet().ShowDialog(this) == DialogResult.OK)
		{
			LocalSettingAccessor.ReadCoilConstant(out var num, out var num2, out var num3);
			coilConstantX = num;
			coilConstantY = num2;
			coilConstantZ = num3;
		}
	}

	private void toolStripButton4_Click(object sender, EventArgs e)
	{
		if (toolStripButton4.Text.Equals("数据保存"))
		{
			FormSaveDirSet formSaveDirSet = new FormSaveDirSet();
			if (formSaveDirSet.ShowDialog(this) == DialogResult.OK)
			{
				toolStripButton4.Text = "保存中...";
				toolStripButton4.Image = Resources.红灯;
				saveLedRed = true;
				timer2.Start();
				savePath = formSaveDirSet.SavePath;
				timer1.Interval = (int)(formSaveDirSet.SamplingInterval * 1000.0);
				save_Flag = true;
				dataWriter = File.AppendText(savePath);
			}
		}
		else
		{
			toolStripButton4.Text = "数据保存";
			toolStripButton4.Image = Resources.保存;
			saveLedRed = false;
			timer1.Interval = 500;
			timer2.Stop();
			save_Flag = false;
			dataWriter.Close();
		}
	}

	private void toolStripButton5_Click(object sender, EventArgs e)
	{
		new AboutBox1().ShowDialog(this);
	}

	private void toolStripButton6_Click(object sender, EventArgs e)
	{
		string text = "磁场控制系统软件使用说明.pdf";
		if (!File.Exists(text))
		{
			MessageBox.Show(this, "找不到帮助文档！", "错误");
		}
		else
		{
			Process.Start(text);
		}
	}

	private void cToggleButtonOutputX_CheckedChanged(object sender, EventArgs e)
	{
		if (cToggleButtonOutputX.Checked)
		{
			double num = ((!cToggleButtonLockZeroX.Checked) ? zeroSetCurrX.Value : (zeroSetCurrX.Value + recurSetCurrX.Value));
			if (num - POWER_MAX_CURR > 0.001)
			{
				MessageBox.Show("设置电流超出了电源输出范围！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			SetPowerCurr(recurrencePorts[0], num);
			recurrencePorts[0].Write("OUTP 1\n");
			cToggleButtonLockZeroX.Enabled = true;
		}
		else
		{
			SetPowerCurr(recurrencePorts[0], 0.0);
			recurrencePorts[0].Write("OUTP 0\n");
			cToggleButtonLockZeroX.Enabled = false;
		}
	}

	private void cToggleButtonOutputY_CheckedChanged(object sender, EventArgs e)
	{
		if (cToggleButtonOutputY.Checked)
		{
			double num = ((!cToggleButtonLockZeroY.Checked) ? zeroSetCurrY.Value : (zeroSetCurrY.Value + recurSetCurrY.Value));
			if (num - POWER_MAX_CURR > 0.001)
			{
				MessageBox.Show("设置电流超出了电源输出范围！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			SetPowerCurr(recurrencePorts[1], num);
			recurrencePorts[1].Write("OUTP 1\n");
			cToggleButtonLockZeroY.Enabled = true;
		}
		else
		{
			recurrencePorts[1].Write("OUTP 0\n");
			cToggleButtonLockZeroY.Enabled = false;
		}
	}

	private void cToggleButtonOutputZ_CheckedChanged(object sender, EventArgs e)
	{
		if (cToggleButtonOutputZ.Checked)
		{
			double num = ((!cToggleButtonLockZeroZ.Checked) ? zeroSetCurrZ.Value : (zeroSetCurrZ.Value + recurSetCurrZ.Value));
			if (num - POWER_MAX_CURR > 0.001)
			{
				MessageBox.Show("设置电流超出了电源输出范围！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			SetPowerCurr(recurrencePorts[2], num);
			recurrencePorts[2].Write("OUTP 1\n");
			cToggleButtonLockZeroZ.Enabled = true;
		}
		else
		{
			recurrencePorts[2].Write("OUTP 0\n");
			cToggleButtonLockZeroZ.Enabled = false;
		}
	}

	private void cToggleButtonLockZeroX_CheckedChanged(object sender, EventArgs e)
	{
		if (cToggleButtonLockZeroX.Checked)
		{
			double num = zeroSetCurrX.Value + recurSetCurrX.Value;
			if (num - POWER_MAX_CURR > 0.001)
			{
				MessageBox.Show("设置电流超出了电源输出范围！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			SetPowerCurr(recurrencePorts[0], num);
			zeroSetCurrX.Enabled = false;
		}
		else
		{
			double value = zeroSetCurrX.Value;
			SetPowerCurr(recurrencePorts[0], value);
			zeroSetCurrX.Enabled = true;
		}
	}

	private void cToggleButtonLockZeroY_CheckedChanged(object sender, EventArgs e)
	{
		if (cToggleButtonLockZeroY.Checked)
		{
			double num = zeroSetCurrY.Value + recurSetCurrY.Value;
			if (num - POWER_MAX_CURR > 0.001)
			{
				MessageBox.Show("设置电流超出了电源输出范围！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			SetPowerCurr(recurrencePorts[1], num);
			zeroSetCurrY.Enabled = false;
		}
		else
		{
			double value = zeroSetCurrY.Value;
			SetPowerCurr(recurrencePorts[1], value);
			zeroSetCurrY.Enabled = true;
		}
	}

	private void cToggleButtonLockZeroZ_CheckedChanged(object sender, EventArgs e)
	{
		if (cToggleButtonLockZeroZ.Checked)
		{
			double num = zeroSetCurrZ.Value + recurSetCurrZ.Value;
			if (num - POWER_MAX_CURR > 0.001)
			{
				MessageBox.Show("设置电流超出了电源输出范围！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			SetPowerCurr(recurrencePorts[2], num);
			zeroSetCurrZ.Enabled = false;
		}
		else
		{
			double value = zeroSetCurrZ.Value;
			SetPowerCurr(recurrencePorts[2], value);
			zeroSetCurrZ.Enabled = true;
		}
	}

	private void RecurrenceX_DataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		BeginInvoke((MethodInvoker)delegate
		{
			SerialPort serialPort = (SerialPort)sender;
			if (serialPort == null || !serialPort.IsOpen || serialPort.BytesToRead <= 0)
			{
				return;
			}
			try
			{
				string text = serialPort.ReadLine();
				if (!string.IsNullOrWhiteSpace(text))
				{
					if (double.TryParse(text, out var result))
					{
						textBoxTotalCurrX.Text = (result * 1000.0).ToString("f2");
						if (cToggleButtonLockZeroX.Checked)
						{
							if (cToggleButtonOutputX.Checked)
							{
								double num = result * 1000.0 - double.Parse(textBoxZeroCurrX.Text);
								double num2 = num * coilConstantX;
								textBoxRecurCurrX.Text = num.ToString("f2");
								textBoxRecurMagX.Text = num2.ToString("f2");
							}
							else
							{
								textBoxRecurCurrX.Text = string.Empty;
								textBoxRecurMagX.Text = string.Empty;
							}
						}
						else
						{
							textBoxZeroCurrX.Text = (result * 1000.0).ToString("f2");
							textBoxRecurCurrX.Text = string.Empty;
							textBoxRecurMagX.Text = string.Empty;
						}
					}
					else
					{
						textBoxTotalCurrX.Text = string.Empty;
						if (cToggleButtonLockZeroX.Checked)
						{
							textBoxRecurCurrX.Text = string.Empty;
							textBoxRecurMagX.Text = string.Empty;
						}
						else
						{
							textBoxZeroCurrX.Text = string.Empty;
							textBoxRecurCurrX.Text = string.Empty;
							textBoxRecurMagX.Text = string.Empty;
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorLogger.Write(ex.Message);
			}
		});
	}

	private void RecurrenceY_DataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		BeginInvoke((MethodInvoker)delegate
		{
			SerialPort serialPort = (SerialPort)sender;
			if (serialPort == null || !serialPort.IsOpen || serialPort.BytesToRead <= 0)
			{
				return;
			}
			try
			{
				string text = serialPort.ReadLine();
				if (!string.IsNullOrWhiteSpace(text))
				{
					if (double.TryParse(text, out var result))
					{
						textBoxTotalCurrY.Text = (result * 1000.0).ToString("f2");
						if (cToggleButtonLockZeroY.Checked)
						{
							if (cToggleButtonOutputY.Checked)
							{
								double num = result * 1000.0 - double.Parse(textBoxZeroCurrY.Text);
								double num2 = num * coilConstantY;
								textBoxRecurCurrY.Text = num.ToString("f2");
								textBoxRecurMagY.Text = num2.ToString("f2");
							}
							else
							{
								textBoxRecurCurrY.Text = string.Empty;
								textBoxRecurMagY.Text = string.Empty;
							}
						}
						else
						{
							textBoxZeroCurrY.Text = (result * 1000.0).ToString("f2");
							textBoxRecurCurrY.Text = string.Empty;
							textBoxRecurMagY.Text = string.Empty;
						}
					}
					else
					{
						textBoxTotalCurrY.Text = string.Empty;
						if (cToggleButtonLockZeroY.Checked)
						{
							textBoxRecurCurrY.Text = string.Empty;
							textBoxRecurMagY.Text = string.Empty;
						}
						else
						{
							textBoxZeroCurrY.Text = string.Empty;
							textBoxRecurCurrY.Text = string.Empty;
							textBoxRecurMagY.Text = string.Empty;
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorLogger.Write(ex.Message);
			}
		});
	}

	private void RecurrenceZ_DataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		BeginInvoke((MethodInvoker)delegate
		{
			SerialPort serialPort = (SerialPort)sender;
			if (serialPort == null || !serialPort.IsOpen || serialPort.BytesToRead <= 0)
			{
				return;
			}
			try
			{
				string text = serialPort.ReadLine();
				if (!string.IsNullOrWhiteSpace(text))
				{
					if (double.TryParse(text, out var result))
					{
						textBoxTotalCurrZ.Text = (result * 1000.0).ToString("f2");
						if (cToggleButtonLockZeroZ.Checked)
						{
							if (cToggleButtonOutputZ.Checked)
							{
								double num = result * 1000.0 - double.Parse(textBoxZeroCurrZ.Text);
								double num2 = num * coilConstantZ;
								textBoxRecurCurrZ.Text = num.ToString("f2");
								textBoxRecurMagZ.Text = num2.ToString("f2");
							}
							else
							{
								textBoxRecurCurrZ.Text = string.Empty;
								textBoxRecurMagZ.Text = string.Empty;
							}
						}
						else
						{
							textBoxZeroCurrZ.Text = (result * 1000.0).ToString("f2");
							textBoxRecurCurrZ.Text = string.Empty;
							textBoxRecurMagZ.Text = string.Empty;
						}
					}
					else
					{
						textBoxTotalCurrZ.Text = string.Empty;
						if (cToggleButtonLockZeroZ.Checked)
						{
							textBoxRecurCurrZ.Text = string.Empty;
							textBoxRecurMagZ.Text = string.Empty;
						}
						else
						{
							textBoxZeroCurrZ.Text = string.Empty;
							textBoxRecurCurrZ.Text = string.Empty;
							textBoxRecurMagZ.Text = string.Empty;
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorLogger.Write(ex.Message);
			}
		});
	}

	private bool checkHardware()
	{
		LocalSettingAccessor.ReadCommports(out var ports, out var baudRates);
		for (int i = 0; i < 3; i++)
		{
			try
			{
				recurrencePorts[i] = new SerialPort();
				recurrencePorts[i].PortName = ports[i];
				recurrencePorts[i].BaudRate = baudRates[i];
				recurrencePorts[i].Parity = Parity.None;
				recurrencePorts[i].StopBits = StopBits.One;
				recurrencePorts[i].DataBits = 8;
				recurrencePorts[i].Handshake = Handshake.None;
				recurrencePorts[i].DtrEnable = true;
				recurrencePorts[i].ReadTimeout = 100;
				recurrencePorts[i].Open();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"{recurrencePorts[i].PortName}打开失败！\n" + ex.Message, "异常", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				while (i > 0)
				{
					recurrencePorts[--i].Close();
				}
				return false;
			}
		}
		Thread.Sleep(100);
		SerialPort[] array = recurrencePorts;
		foreach (SerialPort obj in array)
		{
			obj.DiscardInBuffer();
			obj.Write("*IDN?\n");
		}
		Thread.Sleep(100);
		for (int k = 0; k < 3; k++)
		{
			string text = recurrencePorts[k].ReadExisting();
			if (string.IsNullOrEmpty(text) || text.Split(',').Length < 2)
			{
				MessageBox.Show($"{recurrencePorts[k].PortName}未收到响应！");
				array = recurrencePorts;
				for (int j = 0; j < array.Length; j++)
				{
					array[j].Close();
				}
				return false;
			}
		}
		return true;
	}

	private void SetPowerCurr(SerialPort sp, double totalCurr)
	{
		string text = $"CURR {Math.Abs(totalCurr) / 1000.0:f5}\n";
		sp.Write(text);
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		SerialPort[] array = recurrencePorts;
		foreach (SerialPort serialPort in array)
		{
			if (serialPort.IsOpen)
			{
				serialPort.Write("MEAS:CURR?\n");
			}
		}
	}

	private void timer2_Tick(object sender, EventArgs e)
	{
		if (saveLedRed)
		{
			toolStripButton4.Image = Resources.透明灯;
			saveLedRed = false;
			if (save_Flag)
			{
				double num = double.Parse(textBoxZeroCurrX.Text);
				double num2 = double.Parse(textBoxRecurCurrX.Text);
				double num3 = double.Parse(textBoxRecurMagX.Text);
				double num4 = double.Parse(textBoxZeroCurrY.Text);
				double num5 = double.Parse(textBoxRecurCurrY.Text);
				double num6 = double.Parse(textBoxRecurMagY.Text);
				double num7 = double.Parse(textBoxZeroCurrZ.Text);
				double num8 = double.Parse(textBoxRecurCurrZ.Text);
				double num9 = double.Parse(textBoxRecurMagZ.Text);
				string value = $"{DateTime.Now:yyyyMMddHHmmss},{num:f2},{num2:f2},{num3:f2},{num4:f2},{num5:f2},{num6:f2},{num7:f2},{num8:f2},{num9:f2}";
				dataWriter.WriteLine(value);
				dataWriter.Flush();
			}
		}
		else
		{
			toolStripButton4.Image = Resources.红灯;
			saveLedRed = true;
		}
	}

	private void zeroSetCurrX_ValueChanged(object sender, double e)
	{
		if (cToggleButtonOutputX.Checked)
		{
			double value = zeroSetCurrX.Value;
			SetPowerCurr(recurrencePorts[0], value);
		}
	}

	private void recurSetCurrX_ValueChanged(object sender, double e)
	{
		double num = zeroSetCurrX.Value + recurSetCurrX.Value;
		if (cToggleButtonOutputX.Checked && cToggleButtonLockZeroX.Checked)
		{
			if (num - POWER_MAX_CURR > 0.001)
			{
				MessageBox.Show("总电流超范围！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				double num2 = POWER_MAX_CURR - zeroSetCurrX.Value;
				recurSetCurrX.Value = num2;
				recurSetMagX.Value = num2 * coilConstantX;
				SetPowerCurr(recurrencePorts[0], POWER_MAX_CURR);
			}
			else
			{
				recurSetMagX.Value = recurSetCurrX.Value * coilConstantX;
				SetPowerCurr(recurrencePorts[0], num);
			}
		}
		else
		{
			recurSetMagX.Value = recurSetCurrX.Value * coilConstantX;
		}
	}

	private void magSetValueX_ValueChanged(object sender, double e)
	{
		double num = recurSetMagX.Value / coilConstantX;
		if (cToggleButtonOutputX.Checked && cToggleButtonLockZeroX.Checked)
		{
			double num2 = zeroSetCurrX.Value + num;
			if (num2 - POWER_MAX_CURR > 0.001)
			{
				MessageBox.Show("总电流超范围！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				num = POWER_MAX_CURR - zeroSetCurrX.Value;
				recurSetCurrX.Value = num;
				recurSetMagX.Value = num * coilConstantX;
				SetPowerCurr(recurrencePorts[0], POWER_MAX_CURR);
			}
			else
			{
				recurSetCurrX.Value = num;
				SetPowerCurr(recurrencePorts[0], num2);
			}
		}
		else if (num - POWER_MAX_CURR > 0.001)
		{
			MessageBox.Show("复现电流超范围！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			recurSetCurrX.Value = POWER_MAX_CURR;
			recurSetMagX.Value = POWER_MAX_CURR * coilConstantX;
		}
		else
		{
			recurSetCurrX.Value = num;
		}
	}

	private void zeroSetCurrY_ValueChanged(object sender, double e)
	{
		if (cToggleButtonOutputY.Checked)
		{
			double value = zeroSetCurrY.Value;
			SetPowerCurr(recurrencePorts[1], value);
		}
	}

	private void recurSetCurrY_ValueChanged(object sender, double e)
	{
		double num = zeroSetCurrY.Value + recurSetCurrY.Value;
		if (cToggleButtonOutputY.Checked && cToggleButtonLockZeroY.Checked)
		{
			if (num - POWER_MAX_CURR > 0.001)
			{
				MessageBox.Show("总电流超范围！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				double num2 = POWER_MAX_CURR - zeroSetCurrY.Value;
				recurSetCurrY.Value = num2;
				recurSetMagY.Value = num2 * coilConstantY;
				SetPowerCurr(recurrencePorts[1], POWER_MAX_CURR);
			}
			else
			{
				recurSetMagY.Value = recurSetCurrY.Value * coilConstantY;
				SetPowerCurr(recurrencePorts[1], num);
			}
		}
		else
		{
			recurSetMagY.Value = recurSetCurrY.Value * coilConstantY;
		}
	}

	private void magSetValueY_ValueChanged(object sender, double e)
	{
		double num = recurSetMagY.Value / coilConstantY;
		if (cToggleButtonOutputY.Checked && cToggleButtonLockZeroY.Checked)
		{
			double num2 = zeroSetCurrY.Value + num;
			if (num2 - POWER_MAX_CURR > 0.001)
			{
				MessageBox.Show("总电流超范围！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				num = POWER_MAX_CURR - zeroSetCurrY.Value;
				recurSetCurrY.Value = num;
				recurSetMagY.Value = num * coilConstantY;
				SetPowerCurr(recurrencePorts[1], POWER_MAX_CURR);
			}
			else
			{
				recurSetCurrY.Value = num;
				SetPowerCurr(recurrencePorts[1], num2);
			}
		}
		else if (num - POWER_MAX_CURR > 0.001)
		{
			MessageBox.Show("复现电流超范围！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			recurSetCurrY.Value = POWER_MAX_CURR;
			recurSetMagY.Value = POWER_MAX_CURR * coilConstantY;
		}
		else
		{
			recurSetCurrY.Value = num;
		}
	}

	private void zeroSetCurrZ_ValueChanged(object sender, double e)
	{
		if (cToggleButtonOutputZ.Checked)
		{
			double value = zeroSetCurrZ.Value;
			SetPowerCurr(recurrencePorts[2], value);
		}
	}

	private void recurSetCurrZ_ValueChanged(object sender, double e)
	{
		double num = zeroSetCurrZ.Value + recurSetCurrZ.Value;
		if (cToggleButtonOutputZ.Checked && cToggleButtonLockZeroZ.Checked)
		{
			if (num - POWER_MAX_CURR > 0.001)
			{
				MessageBox.Show("总电流超范围！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				double num2 = POWER_MAX_CURR - zeroSetCurrZ.Value;
				recurSetCurrZ.Value = num2;
				recurSetMagZ.Value = num2 * coilConstantZ;
				SetPowerCurr(recurrencePorts[2], POWER_MAX_CURR);
			}
			else
			{
				recurSetMagZ.Value = recurSetCurrZ.Value * coilConstantZ;
				SetPowerCurr(recurrencePorts[2], num);
			}
		}
		else
		{
			recurSetMagZ.Value = recurSetCurrZ.Value * coilConstantZ;
		}
	}

	private void magSetValueZ_ValueChanged(object sender, double e)
	{
		double num = recurSetMagZ.Value / coilConstantZ;
		if (cToggleButtonOutputZ.Checked && cToggleButtonLockZeroZ.Checked)
		{
			double num2 = zeroSetCurrZ.Value + num;
			if (num2 - POWER_MAX_CURR > 0.001)
			{
				MessageBox.Show("总电流超范围！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				num = POWER_MAX_CURR - zeroSetCurrZ.Value;
				recurSetCurrZ.Value = num;
				recurSetMagZ.Value = num * coilConstantZ;
				SetPowerCurr(recurrencePorts[2], POWER_MAX_CURR);
			}
			else
			{
				recurSetCurrZ.Value = num;
				SetPowerCurr(recurrencePorts[2], num2);
			}
		}
		else if (num - POWER_MAX_CURR > 0.001)
		{
			MessageBox.Show("复现电流超范围！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			recurSetCurrZ.Value = POWER_MAX_CURR;
			recurSetMagZ.Value = POWER_MAX_CURR * coilConstantZ;
		}
		else
		{
			recurSetCurrZ.Value = num;
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
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SimplePowerController.FormMain));
		this.toolStrip1 = new System.Windows.Forms.ToolStrip();
		this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
		this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
		this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
		this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
		this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
		this.toolStripButton6 = new System.Windows.Forms.ToolStripButton();
		this.panel1 = new System.Windows.Forms.Panel();
		this.recurSetMagX = new CControls.CRegulator6x1();
		this.recurSetCurrX = new CControls.CRegulator4x2();
		this.zeroSetCurrX = new CControls.CRegulator4x2();
		this.label7 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.textBoxRecurCurrX = new System.Windows.Forms.TextBox();
		this.label6 = new System.Windows.Forms.Label();
		this.cToggleButtonLockZeroX = new CControls.CToggleButton();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.textBoxZeroCurrX = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		this.cToggleButtonOutputX = new CControls.CToggleButton();
		this.label10 = new System.Windows.Forms.Label();
		this.textBoxRecurMagX = new System.Windows.Forms.TextBox();
		this.label24 = new System.Windows.Forms.Label();
		this.label29 = new System.Windows.Forms.Label();
		this.label62 = new System.Windows.Forms.Label();
		this.label59 = new System.Windows.Forms.Label();
		this.textBoxTotalCurrX = new System.Windows.Forms.TextBox();
		this.label89 = new System.Windows.Forms.Label();
		this.label57 = new System.Windows.Forms.Label();
		this.label58 = new System.Windows.Forms.Label();
		this.panel2 = new System.Windows.Forms.Panel();
		this.recurSetMagY = new CControls.CRegulator6x1();
		this.recurSetCurrY = new CControls.CRegulator4x2();
		this.zeroSetCurrY = new CControls.CRegulator4x2();
		this.label16 = new System.Windows.Forms.Label();
		this.label17 = new System.Windows.Forms.Label();
		this.label18 = new System.Windows.Forms.Label();
		this.label20 = new System.Windows.Forms.Label();
		this.label27 = new System.Windows.Forms.Label();
		this.label28 = new System.Windows.Forms.Label();
		this.label11 = new System.Windows.Forms.Label();
		this.label12 = new System.Windows.Forms.Label();
		this.textBoxRecurCurrY = new System.Windows.Forms.TextBox();
		this.label13 = new System.Windows.Forms.Label();
		this.cToggleButtonLockZeroY = new CControls.CToggleButton();
		this.label14 = new System.Windows.Forms.Label();
		this.label15 = new System.Windows.Forms.Label();
		this.textBoxZeroCurrY = new System.Windows.Forms.TextBox();
		this.label19 = new System.Windows.Forms.Label();
		this.cToggleButtonOutputY = new CControls.CToggleButton();
		this.textBoxRecurMagY = new System.Windows.Forms.TextBox();
		this.label21 = new System.Windows.Forms.Label();
		this.label22 = new System.Windows.Forms.Label();
		this.label23 = new System.Windows.Forms.Label();
		this.label25 = new System.Windows.Forms.Label();
		this.textBoxTotalCurrY = new System.Windows.Forms.TextBox();
		this.label26 = new System.Windows.Forms.Label();
		this.panel3 = new System.Windows.Forms.Panel();
		this.recurSetMagZ = new CControls.CRegulator6x1();
		this.label30 = new System.Windows.Forms.Label();
		this.recurSetCurrZ = new CControls.CRegulator4x2();
		this.zeroSetCurrZ = new CControls.CRegulator4x2();
		this.label31 = new System.Windows.Forms.Label();
		this.label35 = new System.Windows.Forms.Label();
		this.textBoxRecurCurrZ = new System.Windows.Forms.TextBox();
		this.label36 = new System.Windows.Forms.Label();
		this.label32 = new System.Windows.Forms.Label();
		this.label37 = new System.Windows.Forms.Label();
		this.cToggleButtonLockZeroZ = new CControls.CToggleButton();
		this.label39 = new System.Windows.Forms.Label();
		this.label33 = new System.Windows.Forms.Label();
		this.label45 = new System.Windows.Forms.Label();
		this.label34 = new System.Windows.Forms.Label();
		this.label46 = new System.Windows.Forms.Label();
		this.textBoxZeroCurrZ = new System.Windows.Forms.TextBox();
		this.label38 = new System.Windows.Forms.Label();
		this.cToggleButtonOutputZ = new CControls.CToggleButton();
		this.textBoxRecurMagZ = new System.Windows.Forms.TextBox();
		this.label40 = new System.Windows.Forms.Label();
		this.label41 = new System.Windows.Forms.Label();
		this.label42 = new System.Windows.Forms.Label();
		this.label43 = new System.Windows.Forms.Label();
		this.textBoxTotalCurrZ = new System.Windows.Forms.TextBox();
		this.label44 = new System.Windows.Forms.Label();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.timer2 = new System.Windows.Forms.Timer(this.components);
		this.toolStrip1.SuspendLayout();
		this.panel1.SuspendLayout();
		this.panel2.SuspendLayout();
		this.panel3.SuspendLayout();
		base.SuspendLayout();
		this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
		this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripButton1, this.toolStripButton2, this.toolStripButton3, this.toolStripButton4, this.toolStripButton5, this.toolStripButton6 });
		this.toolStrip1.Location = new System.Drawing.Point(0, 0);
		this.toolStrip1.Name = "toolStrip1";
		this.toolStrip1.Size = new System.Drawing.Size(1085, 31);
		this.toolStrip1.TabIndex = 1;
		this.toolStrip1.Text = "toolStrip1";
		this.toolStripButton1.Font = new System.Drawing.Font("新宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.toolStripButton1.Image = (System.Drawing.Image)resources.GetObject("toolStripButton1.Image");
		this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.toolStripButton1.Name = "toolStripButton1";
		this.toolStripButton1.Size = new System.Drawing.Size(91, 28);
		this.toolStripButton1.Text = "连接设备";
		this.toolStripButton1.Click += new System.EventHandler(toolStripButton1_Click);
		this.toolStripButton2.Font = new System.Drawing.Font("新宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.toolStripButton2.Image = (System.Drawing.Image)resources.GetObject("toolStripButton2.Image");
		this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.toolStripButton2.Name = "toolStripButton2";
		this.toolStripButton2.Size = new System.Drawing.Size(91, 28);
		this.toolStripButton2.Text = "连接设置";
		this.toolStripButton2.Click += new System.EventHandler(toolStripButton2_Click);
		this.toolStripButton3.Font = new System.Drawing.Font("新宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.toolStripButton3.Image = (System.Drawing.Image)resources.GetObject("toolStripButton3.Image");
		this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.toolStripButton3.Name = "toolStripButton3";
		this.toolStripButton3.Size = new System.Drawing.Size(91, 28);
		this.toolStripButton3.Text = "参数设置";
		this.toolStripButton3.Click += new System.EventHandler(toolStripButton3_Click);
		this.toolStripButton4.Font = new System.Drawing.Font("新宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.toolStripButton4.Image = (System.Drawing.Image)resources.GetObject("toolStripButton4.Image");
		this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.toolStripButton4.Name = "toolStripButton4";
		this.toolStripButton4.Size = new System.Drawing.Size(91, 28);
		this.toolStripButton4.Text = "数据保存";
		this.toolStripButton4.Click += new System.EventHandler(toolStripButton4_Click);
		this.toolStripButton5.Image = (System.Drawing.Image)resources.GetObject("toolStripButton5.Image");
		this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.toolStripButton5.Name = "toolStripButton5";
		this.toolStripButton5.Size = new System.Drawing.Size(60, 28);
		this.toolStripButton5.Text = "关于";
		this.toolStripButton5.Click += new System.EventHandler(toolStripButton5_Click);
		this.toolStripButton6.Image = (System.Drawing.Image)resources.GetObject("toolStripButton6.Image");
		this.toolStripButton6.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.toolStripButton6.Name = "toolStripButton6";
		this.toolStripButton6.Size = new System.Drawing.Size(60, 28);
		this.toolStripButton6.Text = "帮助";
		this.toolStripButton6.Click += new System.EventHandler(toolStripButton6_Click);
		this.panel1.BackColor = System.Drawing.Color.Lavender;
		this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel1.Controls.Add(this.recurSetMagX);
		this.panel1.Controls.Add(this.recurSetCurrX);
		this.panel1.Controls.Add(this.zeroSetCurrX);
		this.panel1.Controls.Add(this.label7);
		this.panel1.Controls.Add(this.label9);
		this.panel1.Controls.Add(this.textBoxRecurCurrX);
		this.panel1.Controls.Add(this.label6);
		this.panel1.Controls.Add(this.cToggleButtonLockZeroX);
		this.panel1.Controls.Add(this.label4);
		this.panel1.Controls.Add(this.label5);
		this.panel1.Controls.Add(this.textBoxZeroCurrX);
		this.panel1.Controls.Add(this.label3);
		this.panel1.Controls.Add(this.label2);
		this.panel1.Controls.Add(this.label1);
		this.panel1.Controls.Add(this.label8);
		this.panel1.Controls.Add(this.cToggleButtonOutputX);
		this.panel1.Controls.Add(this.label10);
		this.panel1.Controls.Add(this.textBoxRecurMagX);
		this.panel1.Controls.Add(this.label24);
		this.panel1.Controls.Add(this.label29);
		this.panel1.Controls.Add(this.label62);
		this.panel1.Controls.Add(this.label59);
		this.panel1.Controls.Add(this.textBoxTotalCurrX);
		this.panel1.Controls.Add(this.label89);
		this.panel1.Controls.Add(this.label57);
		this.panel1.Controls.Add(this.label58);
		this.panel1.Location = new System.Drawing.Point(12, 34);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(1063, 237);
		this.panel1.TabIndex = 153;
		this.recurSetMagX.ColorBackground = System.Drawing.Color.Black;
		this.recurSetMagX.ColorDark = System.Drawing.Color.FromArgb(0, 64, 0);
		this.recurSetMagX.ColorLight = System.Drawing.Color.Lime;
		this.recurSetMagX.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.recurSetMagX.Location = new System.Drawing.Point(819, 19);
		this.recurSetMagX.Margin = new System.Windows.Forms.Padding(4);
		this.recurSetMagX.MaxValue = 999999.0;
		this.recurSetMagX.MinValue = 0.0;
		this.recurSetMagX.Name = "recurSetMagX";
		this.recurSetMagX.Size = new System.Drawing.Size(195, 97);
		this.recurSetMagX.TabIndex = 218;
		this.recurSetMagX.Value = 0.0;
		this.recurSetMagX.ValueChanged += new System.EventHandler<double>(magSetValueX_ValueChanged);
		this.recurSetCurrX.ColorBackground = System.Drawing.Color.Black;
		this.recurSetCurrX.ColorDark = System.Drawing.Color.FromArgb(80, 0, 0);
		this.recurSetCurrX.ColorLight = System.Drawing.Color.Red;
		this.recurSetCurrX.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.recurSetCurrX.Location = new System.Drawing.Point(522, 18);
		this.recurSetCurrX.Margin = new System.Windows.Forms.Padding(4);
		this.recurSetCurrX.MaxValue = 5000.0;
		this.recurSetCurrX.MinValue = 0.0;
		this.recurSetCurrX.Name = "recurSetCurrX";
		this.recurSetCurrX.Size = new System.Drawing.Size(169, 98);
		this.recurSetCurrX.TabIndex = 217;
		this.recurSetCurrX.Value = 0.0;
		this.recurSetCurrX.ValueChanged += new System.EventHandler<double>(recurSetCurrX_ValueChanged);
		this.zeroSetCurrX.ColorBackground = System.Drawing.Color.Black;
		this.zeroSetCurrX.ColorDark = System.Drawing.Color.FromArgb(80, 0, 0);
		this.zeroSetCurrX.ColorLight = System.Drawing.Color.Red;
		this.zeroSetCurrX.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.zeroSetCurrX.Location = new System.Drawing.Point(201, 18);
		this.zeroSetCurrX.Margin = new System.Windows.Forms.Padding(4);
		this.zeroSetCurrX.MaxValue = 5000.0;
		this.zeroSetCurrX.MinValue = 0.0;
		this.zeroSetCurrX.Name = "zeroSetCurrX";
		this.zeroSetCurrX.Size = new System.Drawing.Size(169, 98);
		this.zeroSetCurrX.TabIndex = 216;
		this.zeroSetCurrX.Value = 0.0;
		this.zeroSetCurrX.ValueChanged += new System.EventHandler<double>(zeroSetCurrX_ValueChanged);
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(653, 137);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(21, 14);
		this.label7.TabIndex = 215;
		this.label7.Text = "mA";
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(415, 137);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(147, 14);
		this.label9.TabIndex = 214;
		this.label9.Text = "复现磁场电流计算值：";
		this.textBoxRecurCurrX.Location = new System.Drawing.Point(573, 133);
		this.textBoxRecurCurrX.Name = "textBoxRecurCurrX";
		this.textBoxRecurCurrX.ReadOnly = true;
		this.textBoxRecurCurrX.Size = new System.Drawing.Size(75, 23);
		this.textBoxRecurCurrX.TabIndex = 213;
		this.label6.AutoSize = true;
		this.label6.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label6.Location = new System.Drawing.Point(663, 194);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(67, 14);
		this.label6.TabIndex = 212;
		this.label6.Text = "锁定零场";
		this.cToggleButtonLockZeroX.AutoSize = true;
		this.cToggleButtonLockZeroX.Enabled = false;
		this.cToggleButtonLockZeroX.Location = new System.Drawing.Point(741, 190);
		this.cToggleButtonLockZeroX.MinimumSize = new System.Drawing.Size(45, 22);
		this.cToggleButtonLockZeroX.Name = "cToggleButtonLockZeroX";
		this.cToggleButtonLockZeroX.OffBackColor = System.Drawing.Color.Gray;
		this.cToggleButtonLockZeroX.OffToggleColor = System.Drawing.Color.Gainsboro;
		this.cToggleButtonLockZeroX.OnBackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		this.cToggleButtonLockZeroX.OnToggleColor = System.Drawing.Color.WhiteSmoke;
		this.cToggleButtonLockZeroX.Size = new System.Drawing.Size(45, 22);
		this.cToggleButtonLockZeroX.TabIndex = 211;
		this.cToggleButtonLockZeroX.UseVisualStyleBackColor = true;
		this.cToggleButtonLockZeroX.CheckedChanged += new System.EventHandler(cToggleButtonLockZeroX_CheckedChanged);
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(321, 137);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(21, 14);
		this.label4.TabIndex = 210;
		this.label4.Text = "mA";
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(91, 137);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(147, 14);
		this.label5.TabIndex = 209;
		this.label5.Text = "零场偏置电流回读值：";
		this.textBoxZeroCurrX.Location = new System.Drawing.Point(242, 133);
		this.textBoxZeroCurrX.Name = "textBoxZeroCurrX";
		this.textBoxZeroCurrX.ReadOnly = true;
		this.textBoxZeroCurrX.Size = new System.Drawing.Size(75, 23);
		this.textBoxZeroCurrX.TabIndex = 208;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(1016, 60);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(21, 14);
		this.label3.TabIndex = 207;
		this.label3.Text = "nT";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(741, 60);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(77, 14);
		this.label2.TabIndex = 206;
		this.label2.Text = "复现磁场：";
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(691, 60);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(21, 14);
		this.label1.TabIndex = 202;
		this.label1.Text = "mA";
		this.label8.AutoSize = true;
		this.label8.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label8.Location = new System.Drawing.Point(856, 194);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(37, 14);
		this.label8.TabIndex = 199;
		this.label8.Text = "输出";
		this.cToggleButtonOutputX.AutoSize = true;
		this.cToggleButtonOutputX.Enabled = false;
		this.cToggleButtonOutputX.Location = new System.Drawing.Point(905, 190);
		this.cToggleButtonOutputX.MinimumSize = new System.Drawing.Size(45, 22);
		this.cToggleButtonOutputX.Name = "cToggleButtonOutputX";
		this.cToggleButtonOutputX.OffBackColor = System.Drawing.Color.Gray;
		this.cToggleButtonOutputX.OffToggleColor = System.Drawing.Color.Gainsboro;
		this.cToggleButtonOutputX.OnBackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		this.cToggleButtonOutputX.OnToggleColor = System.Drawing.Color.WhiteSmoke;
		this.cToggleButtonOutputX.Size = new System.Drawing.Size(45, 22);
		this.cToggleButtonOutputX.TabIndex = 198;
		this.cToggleButtonOutputX.UseVisualStyleBackColor = true;
		this.cToggleButtonOutputX.CheckedChanged += new System.EventHandler(cToggleButtonOutputX_CheckedChanged);
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(415, 60);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(105, 14);
		this.label10.TabIndex = 179;
		this.label10.Text = "复现磁场电流：";
		this.textBoxRecurMagX.Location = new System.Drawing.Point(866, 133);
		this.textBoxRecurMagX.Name = "textBoxRecurMagX";
		this.textBoxRecurMagX.ReadOnly = true;
		this.textBoxRecurMagX.Size = new System.Drawing.Size(84, 23);
		this.textBoxRecurMagX.TabIndex = 157;
		this.label24.AutoSize = true;
		this.label24.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.label24.Location = new System.Drawing.Point(742, 137);
		this.label24.Name = "label24";
		this.label24.Size = new System.Drawing.Size(119, 14);
		this.label24.TabIndex = 156;
		this.label24.Text = "复现磁场计算值：";
		this.label29.AutoSize = true;
		this.label29.Location = new System.Drawing.Point(957, 137);
		this.label29.Name = "label29";
		this.label29.Size = new System.Drawing.Size(21, 14);
		this.label29.TabIndex = 158;
		this.label29.Text = "nT";
		this.label62.AutoSize = true;
		this.label62.Location = new System.Drawing.Point(320, 171);
		this.label62.Name = "label62";
		this.label62.Size = new System.Drawing.Size(21, 14);
		this.label62.TabIndex = 155;
		this.label62.Text = "mA";
		this.label59.AutoSize = true;
		this.label59.Location = new System.Drawing.Point(91, 170);
		this.label59.Name = "label59";
		this.label59.Size = new System.Drawing.Size(133, 14);
		this.label59.TabIndex = 154;
		this.label59.Text = "输出总电流回读值：";
		this.textBoxTotalCurrX.Location = new System.Drawing.Point(242, 167);
		this.textBoxTotalCurrX.Name = "textBoxTotalCurrX";
		this.textBoxTotalCurrX.ReadOnly = true;
		this.textBoxTotalCurrX.Size = new System.Drawing.Size(75, 23);
		this.textBoxTotalCurrX.TabIndex = 153;
		this.label89.Font = new System.Drawing.Font("Microsoft Sans Serif", 18f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label89.ForeColor = System.Drawing.SystemColors.Highlight;
		this.label89.Location = new System.Drawing.Point(28, 68);
		this.label89.Name = "label89";
		this.label89.Size = new System.Drawing.Size(45, 92);
		this.label89.TabIndex = 9;
		this.label89.Text = "X轴";
		this.label57.AutoSize = true;
		this.label57.Location = new System.Drawing.Point(91, 60);
		this.label57.Name = "label57";
		this.label57.Size = new System.Drawing.Size(105, 14);
		this.label57.TabIndex = 150;
		this.label57.Text = "零场偏置电流：";
		this.label58.AutoSize = true;
		this.label58.Location = new System.Drawing.Point(370, 60);
		this.label58.Name = "label58";
		this.label58.Size = new System.Drawing.Size(21, 14);
		this.label58.TabIndex = 151;
		this.label58.Text = "mA";
		this.panel2.BackColor = System.Drawing.Color.Lavender;
		this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel2.Controls.Add(this.recurSetMagY);
		this.panel2.Controls.Add(this.recurSetCurrY);
		this.panel2.Controls.Add(this.zeroSetCurrY);
		this.panel2.Controls.Add(this.label16);
		this.panel2.Controls.Add(this.label17);
		this.panel2.Controls.Add(this.label18);
		this.panel2.Controls.Add(this.label20);
		this.panel2.Controls.Add(this.label27);
		this.panel2.Controls.Add(this.label28);
		this.panel2.Controls.Add(this.label11);
		this.panel2.Controls.Add(this.label12);
		this.panel2.Controls.Add(this.textBoxRecurCurrY);
		this.panel2.Controls.Add(this.label13);
		this.panel2.Controls.Add(this.cToggleButtonLockZeroY);
		this.panel2.Controls.Add(this.label14);
		this.panel2.Controls.Add(this.label15);
		this.panel2.Controls.Add(this.textBoxZeroCurrY);
		this.panel2.Controls.Add(this.label19);
		this.panel2.Controls.Add(this.cToggleButtonOutputY);
		this.panel2.Controls.Add(this.textBoxRecurMagY);
		this.panel2.Controls.Add(this.label21);
		this.panel2.Controls.Add(this.label22);
		this.panel2.Controls.Add(this.label23);
		this.panel2.Controls.Add(this.label25);
		this.panel2.Controls.Add(this.textBoxTotalCurrY);
		this.panel2.Controls.Add(this.label26);
		this.panel2.Location = new System.Drawing.Point(12, 277);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(1063, 237);
		this.panel2.TabIndex = 154;
		this.recurSetMagY.ColorBackground = System.Drawing.Color.Black;
		this.recurSetMagY.ColorDark = System.Drawing.Color.FromArgb(0, 64, 0);
		this.recurSetMagY.ColorLight = System.Drawing.Color.Lime;
		this.recurSetMagY.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.recurSetMagY.Location = new System.Drawing.Point(819, 18);
		this.recurSetMagY.Margin = new System.Windows.Forms.Padding(4);
		this.recurSetMagY.MaxValue = 999999.0;
		this.recurSetMagY.MinValue = 0.0;
		this.recurSetMagY.Name = "recurSetMagY";
		this.recurSetMagY.Size = new System.Drawing.Size(195, 97);
		this.recurSetMagY.TabIndex = 227;
		this.recurSetMagY.Value = 0.0;
		this.recurSetMagY.ValueChanged += new System.EventHandler<double>(magSetValueY_ValueChanged);
		this.recurSetCurrY.ColorBackground = System.Drawing.Color.Black;
		this.recurSetCurrY.ColorDark = System.Drawing.Color.FromArgb(80, 0, 0);
		this.recurSetCurrY.ColorLight = System.Drawing.Color.Red;
		this.recurSetCurrY.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.recurSetCurrY.Location = new System.Drawing.Point(522, 17);
		this.recurSetCurrY.Margin = new System.Windows.Forms.Padding(4);
		this.recurSetCurrY.MaxValue = 5000.0;
		this.recurSetCurrY.MinValue = 0.0;
		this.recurSetCurrY.Name = "recurSetCurrY";
		this.recurSetCurrY.Size = new System.Drawing.Size(169, 98);
		this.recurSetCurrY.TabIndex = 226;
		this.recurSetCurrY.Value = 0.0;
		this.recurSetCurrY.ValueChanged += new System.EventHandler<double>(recurSetCurrY_ValueChanged);
		this.zeroSetCurrY.ColorBackground = System.Drawing.Color.Black;
		this.zeroSetCurrY.ColorDark = System.Drawing.Color.FromArgb(80, 0, 0);
		this.zeroSetCurrY.ColorLight = System.Drawing.Color.Red;
		this.zeroSetCurrY.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.zeroSetCurrY.Location = new System.Drawing.Point(201, 17);
		this.zeroSetCurrY.Margin = new System.Windows.Forms.Padding(4);
		this.zeroSetCurrY.MaxValue = 5000.0;
		this.zeroSetCurrY.MinValue = 0.0;
		this.zeroSetCurrY.Name = "zeroSetCurrY";
		this.zeroSetCurrY.Size = new System.Drawing.Size(169, 98);
		this.zeroSetCurrY.TabIndex = 225;
		this.zeroSetCurrY.Value = 0.0;
		this.zeroSetCurrY.ValueChanged += new System.EventHandler<double>(zeroSetCurrY_ValueChanged);
		this.label16.AutoSize = true;
		this.label16.Location = new System.Drawing.Point(1016, 59);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(21, 14);
		this.label16.TabIndex = 224;
		this.label16.Text = "nT";
		this.label17.AutoSize = true;
		this.label17.Location = new System.Drawing.Point(741, 59);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(77, 14);
		this.label17.TabIndex = 223;
		this.label17.Text = "复现磁场：";
		this.label18.AutoSize = true;
		this.label18.Location = new System.Drawing.Point(691, 59);
		this.label18.Name = "label18";
		this.label18.Size = new System.Drawing.Size(21, 14);
		this.label18.TabIndex = 222;
		this.label18.Text = "mA";
		this.label20.AutoSize = true;
		this.label20.Location = new System.Drawing.Point(415, 59);
		this.label20.Name = "label20";
		this.label20.Size = new System.Drawing.Size(105, 14);
		this.label20.TabIndex = 221;
		this.label20.Text = "复现磁场电流：";
		this.label27.AutoSize = true;
		this.label27.Location = new System.Drawing.Point(91, 59);
		this.label27.Name = "label27";
		this.label27.Size = new System.Drawing.Size(105, 14);
		this.label27.TabIndex = 219;
		this.label27.Text = "零场偏置电流：";
		this.label28.AutoSize = true;
		this.label28.Location = new System.Drawing.Point(370, 59);
		this.label28.Name = "label28";
		this.label28.Size = new System.Drawing.Size(21, 14);
		this.label28.TabIndex = 220;
		this.label28.Text = "mA";
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(651, 142);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(21, 14);
		this.label11.TabIndex = 215;
		this.label11.Text = "mA";
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(415, 142);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(147, 14);
		this.label12.TabIndex = 214;
		this.label12.Text = "复现磁场电流计算值：";
		this.textBoxRecurCurrY.Location = new System.Drawing.Point(573, 138);
		this.textBoxRecurCurrY.Name = "textBoxRecurCurrY";
		this.textBoxRecurCurrY.ReadOnly = true;
		this.textBoxRecurCurrY.Size = new System.Drawing.Size(75, 23);
		this.textBoxRecurCurrY.TabIndex = 213;
		this.label13.AutoSize = true;
		this.label13.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label13.Location = new System.Drawing.Point(663, 189);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(67, 14);
		this.label13.TabIndex = 212;
		this.label13.Text = "锁定零场";
		this.cToggleButtonLockZeroY.AutoSize = true;
		this.cToggleButtonLockZeroY.Enabled = false;
		this.cToggleButtonLockZeroY.Location = new System.Drawing.Point(741, 185);
		this.cToggleButtonLockZeroY.MinimumSize = new System.Drawing.Size(45, 22);
		this.cToggleButtonLockZeroY.Name = "cToggleButtonLockZeroY";
		this.cToggleButtonLockZeroY.OffBackColor = System.Drawing.Color.Gray;
		this.cToggleButtonLockZeroY.OffToggleColor = System.Drawing.Color.Gainsboro;
		this.cToggleButtonLockZeroY.OnBackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		this.cToggleButtonLockZeroY.OnToggleColor = System.Drawing.Color.WhiteSmoke;
		this.cToggleButtonLockZeroY.Size = new System.Drawing.Size(45, 22);
		this.cToggleButtonLockZeroY.TabIndex = 211;
		this.cToggleButtonLockZeroY.UseVisualStyleBackColor = true;
		this.cToggleButtonLockZeroY.CheckedChanged += new System.EventHandler(cToggleButtonLockZeroY_CheckedChanged);
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(323, 138);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(21, 14);
		this.label14.TabIndex = 210;
		this.label14.Text = "mA";
		this.label15.AutoSize = true;
		this.label15.Location = new System.Drawing.Point(91, 138);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(147, 14);
		this.label15.TabIndex = 209;
		this.label15.Text = "零场偏置电流回读值：";
		this.textBoxZeroCurrY.Location = new System.Drawing.Point(242, 134);
		this.textBoxZeroCurrY.Name = "textBoxZeroCurrY";
		this.textBoxZeroCurrY.ReadOnly = true;
		this.textBoxZeroCurrY.Size = new System.Drawing.Size(75, 23);
		this.textBoxZeroCurrY.TabIndex = 208;
		this.label19.AutoSize = true;
		this.label19.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label19.Location = new System.Drawing.Point(856, 189);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(37, 14);
		this.label19.TabIndex = 199;
		this.label19.Text = "输出";
		this.cToggleButtonOutputY.AutoSize = true;
		this.cToggleButtonOutputY.Enabled = false;
		this.cToggleButtonOutputY.Location = new System.Drawing.Point(905, 185);
		this.cToggleButtonOutputY.MinimumSize = new System.Drawing.Size(45, 22);
		this.cToggleButtonOutputY.Name = "cToggleButtonOutputY";
		this.cToggleButtonOutputY.OffBackColor = System.Drawing.Color.Gray;
		this.cToggleButtonOutputY.OffToggleColor = System.Drawing.Color.Gainsboro;
		this.cToggleButtonOutputY.OnBackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		this.cToggleButtonOutputY.OnToggleColor = System.Drawing.Color.WhiteSmoke;
		this.cToggleButtonOutputY.Size = new System.Drawing.Size(45, 22);
		this.cToggleButtonOutputY.TabIndex = 198;
		this.cToggleButtonOutputY.UseVisualStyleBackColor = true;
		this.cToggleButtonOutputY.CheckedChanged += new System.EventHandler(cToggleButtonOutputY_CheckedChanged);
		this.textBoxRecurMagY.Location = new System.Drawing.Point(866, 134);
		this.textBoxRecurMagY.Name = "textBoxRecurMagY";
		this.textBoxRecurMagY.ReadOnly = true;
		this.textBoxRecurMagY.Size = new System.Drawing.Size(84, 23);
		this.textBoxRecurMagY.TabIndex = 157;
		this.label21.AutoSize = true;
		this.label21.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.label21.Location = new System.Drawing.Point(742, 138);
		this.label21.Name = "label21";
		this.label21.Size = new System.Drawing.Size(119, 14);
		this.label21.TabIndex = 156;
		this.label21.Text = "复现磁场计算值：";
		this.label22.AutoSize = true;
		this.label22.Location = new System.Drawing.Point(957, 138);
		this.label22.Name = "label22";
		this.label22.Size = new System.Drawing.Size(21, 14);
		this.label22.TabIndex = 158;
		this.label22.Text = "nT";
		this.label23.AutoSize = true;
		this.label23.Location = new System.Drawing.Point(320, 172);
		this.label23.Name = "label23";
		this.label23.Size = new System.Drawing.Size(21, 14);
		this.label23.TabIndex = 155;
		this.label23.Text = "mA";
		this.label25.AutoSize = true;
		this.label25.Location = new System.Drawing.Point(91, 171);
		this.label25.Name = "label25";
		this.label25.Size = new System.Drawing.Size(133, 14);
		this.label25.TabIndex = 154;
		this.label25.Text = "输出总电流回读值：";
		this.textBoxTotalCurrY.Location = new System.Drawing.Point(242, 168);
		this.textBoxTotalCurrY.Name = "textBoxTotalCurrY";
		this.textBoxTotalCurrY.ReadOnly = true;
		this.textBoxTotalCurrY.Size = new System.Drawing.Size(75, 23);
		this.textBoxTotalCurrY.TabIndex = 153;
		this.label26.Font = new System.Drawing.Font("Microsoft Sans Serif", 18f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label26.ForeColor = System.Drawing.SystemColors.Highlight;
		this.label26.Location = new System.Drawing.Point(28, 68);
		this.label26.Name = "label26";
		this.label26.Size = new System.Drawing.Size(45, 92);
		this.label26.TabIndex = 9;
		this.label26.Text = "Y轴";
		this.panel3.BackColor = System.Drawing.Color.Lavender;
		this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel3.Controls.Add(this.recurSetMagZ);
		this.panel3.Controls.Add(this.label30);
		this.panel3.Controls.Add(this.recurSetCurrZ);
		this.panel3.Controls.Add(this.zeroSetCurrZ);
		this.panel3.Controls.Add(this.label31);
		this.panel3.Controls.Add(this.label35);
		this.panel3.Controls.Add(this.textBoxRecurCurrZ);
		this.panel3.Controls.Add(this.label36);
		this.panel3.Controls.Add(this.label32);
		this.panel3.Controls.Add(this.label37);
		this.panel3.Controls.Add(this.cToggleButtonLockZeroZ);
		this.panel3.Controls.Add(this.label39);
		this.panel3.Controls.Add(this.label33);
		this.panel3.Controls.Add(this.label45);
		this.panel3.Controls.Add(this.label34);
		this.panel3.Controls.Add(this.label46);
		this.panel3.Controls.Add(this.textBoxZeroCurrZ);
		this.panel3.Controls.Add(this.label38);
		this.panel3.Controls.Add(this.cToggleButtonOutputZ);
		this.panel3.Controls.Add(this.textBoxRecurMagZ);
		this.panel3.Controls.Add(this.label40);
		this.panel3.Controls.Add(this.label41);
		this.panel3.Controls.Add(this.label42);
		this.panel3.Controls.Add(this.label43);
		this.panel3.Controls.Add(this.textBoxTotalCurrZ);
		this.panel3.Controls.Add(this.label44);
		this.panel3.Location = new System.Drawing.Point(12, 520);
		this.panel3.Name = "panel3";
		this.panel3.Size = new System.Drawing.Size(1063, 237);
		this.panel3.TabIndex = 155;
		this.recurSetMagZ.ColorBackground = System.Drawing.Color.Black;
		this.recurSetMagZ.ColorDark = System.Drawing.Color.FromArgb(0, 64, 0);
		this.recurSetMagZ.ColorLight = System.Drawing.Color.Lime;
		this.recurSetMagZ.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.recurSetMagZ.Location = new System.Drawing.Point(819, 18);
		this.recurSetMagZ.Margin = new System.Windows.Forms.Padding(4);
		this.recurSetMagZ.MaxValue = 999999.0;
		this.recurSetMagZ.MinValue = 0.0;
		this.recurSetMagZ.Name = "recurSetMagZ";
		this.recurSetMagZ.Size = new System.Drawing.Size(195, 97);
		this.recurSetMagZ.TabIndex = 236;
		this.recurSetMagZ.Value = 0.0;
		this.recurSetMagZ.ValueChanged += new System.EventHandler<double>(magSetValueZ_ValueChanged);
		this.label30.AutoSize = true;
		this.label30.Location = new System.Drawing.Point(651, 142);
		this.label30.Name = "label30";
		this.label30.Size = new System.Drawing.Size(21, 14);
		this.label30.TabIndex = 215;
		this.label30.Text = "mA";
		this.recurSetCurrZ.ColorBackground = System.Drawing.Color.Black;
		this.recurSetCurrZ.ColorDark = System.Drawing.Color.FromArgb(80, 0, 0);
		this.recurSetCurrZ.ColorLight = System.Drawing.Color.Red;
		this.recurSetCurrZ.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.recurSetCurrZ.Location = new System.Drawing.Point(522, 17);
		this.recurSetCurrZ.Margin = new System.Windows.Forms.Padding(4);
		this.recurSetCurrZ.MaxValue = 5000.0;
		this.recurSetCurrZ.MinValue = 0.0;
		this.recurSetCurrZ.Name = "recurSetCurrZ";
		this.recurSetCurrZ.Size = new System.Drawing.Size(169, 98);
		this.recurSetCurrZ.TabIndex = 235;
		this.recurSetCurrZ.Value = 0.0;
		this.recurSetCurrZ.ValueChanged += new System.EventHandler<double>(recurSetCurrZ_ValueChanged);
		this.zeroSetCurrZ.ColorBackground = System.Drawing.Color.Black;
		this.zeroSetCurrZ.ColorDark = System.Drawing.Color.FromArgb(80, 0, 0);
		this.zeroSetCurrZ.ColorLight = System.Drawing.Color.Red;
		this.zeroSetCurrZ.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.zeroSetCurrZ.Location = new System.Drawing.Point(201, 17);
		this.zeroSetCurrZ.Margin = new System.Windows.Forms.Padding(4);
		this.zeroSetCurrZ.MaxValue = 5000.0;
		this.zeroSetCurrZ.MinValue = 0.0;
		this.zeroSetCurrZ.Name = "zeroSetCurrZ";
		this.zeroSetCurrZ.Size = new System.Drawing.Size(169, 98);
		this.zeroSetCurrZ.TabIndex = 234;
		this.zeroSetCurrZ.Value = 0.0;
		this.zeroSetCurrZ.ValueChanged += new System.EventHandler<double>(zeroSetCurrZ_ValueChanged);
		this.label31.AutoSize = true;
		this.label31.Location = new System.Drawing.Point(415, 142);
		this.label31.Name = "label31";
		this.label31.Size = new System.Drawing.Size(147, 14);
		this.label31.TabIndex = 214;
		this.label31.Text = "复现磁场电流计算值：";
		this.label35.AutoSize = true;
		this.label35.Location = new System.Drawing.Point(1016, 59);
		this.label35.Name = "label35";
		this.label35.Size = new System.Drawing.Size(21, 14);
		this.label35.TabIndex = 233;
		this.label35.Text = "nT";
		this.textBoxRecurCurrZ.Location = new System.Drawing.Point(573, 138);
		this.textBoxRecurCurrZ.Name = "textBoxRecurCurrZ";
		this.textBoxRecurCurrZ.ReadOnly = true;
		this.textBoxRecurCurrZ.Size = new System.Drawing.Size(75, 23);
		this.textBoxRecurCurrZ.TabIndex = 213;
		this.label36.AutoSize = true;
		this.label36.Location = new System.Drawing.Point(741, 59);
		this.label36.Name = "label36";
		this.label36.Size = new System.Drawing.Size(77, 14);
		this.label36.TabIndex = 232;
		this.label36.Text = "复现磁场：";
		this.label32.AutoSize = true;
		this.label32.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label32.Location = new System.Drawing.Point(663, 190);
		this.label32.Name = "label32";
		this.label32.Size = new System.Drawing.Size(67, 14);
		this.label32.TabIndex = 212;
		this.label32.Text = "锁定零场";
		this.label37.AutoSize = true;
		this.label37.Location = new System.Drawing.Point(691, 59);
		this.label37.Name = "label37";
		this.label37.Size = new System.Drawing.Size(21, 14);
		this.label37.TabIndex = 231;
		this.label37.Text = "mA";
		this.cToggleButtonLockZeroZ.AutoSize = true;
		this.cToggleButtonLockZeroZ.Enabled = false;
		this.cToggleButtonLockZeroZ.Location = new System.Drawing.Point(741, 186);
		this.cToggleButtonLockZeroZ.MinimumSize = new System.Drawing.Size(45, 22);
		this.cToggleButtonLockZeroZ.Name = "cToggleButtonLockZeroZ";
		this.cToggleButtonLockZeroZ.OffBackColor = System.Drawing.Color.Gray;
		this.cToggleButtonLockZeroZ.OffToggleColor = System.Drawing.Color.Gainsboro;
		this.cToggleButtonLockZeroZ.OnBackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		this.cToggleButtonLockZeroZ.OnToggleColor = System.Drawing.Color.WhiteSmoke;
		this.cToggleButtonLockZeroZ.Size = new System.Drawing.Size(45, 22);
		this.cToggleButtonLockZeroZ.TabIndex = 211;
		this.cToggleButtonLockZeroZ.UseVisualStyleBackColor = true;
		this.cToggleButtonLockZeroZ.CheckedChanged += new System.EventHandler(cToggleButtonLockZeroZ_CheckedChanged);
		this.label39.AutoSize = true;
		this.label39.Location = new System.Drawing.Point(415, 59);
		this.label39.Name = "label39";
		this.label39.Size = new System.Drawing.Size(105, 14);
		this.label39.TabIndex = 230;
		this.label39.Text = "复现磁场电流：";
		this.label33.AutoSize = true;
		this.label33.Location = new System.Drawing.Point(323, 138);
		this.label33.Name = "label33";
		this.label33.Size = new System.Drawing.Size(21, 14);
		this.label33.TabIndex = 210;
		this.label33.Text = "mA";
		this.label45.AutoSize = true;
		this.label45.Location = new System.Drawing.Point(91, 59);
		this.label45.Name = "label45";
		this.label45.Size = new System.Drawing.Size(105, 14);
		this.label45.TabIndex = 228;
		this.label45.Text = "零场偏置电流：";
		this.label34.AutoSize = true;
		this.label34.Location = new System.Drawing.Point(91, 138);
		this.label34.Name = "label34";
		this.label34.Size = new System.Drawing.Size(147, 14);
		this.label34.TabIndex = 209;
		this.label34.Text = "零场偏置电流回读值：";
		this.label46.AutoSize = true;
		this.label46.Location = new System.Drawing.Point(370, 59);
		this.label46.Name = "label46";
		this.label46.Size = new System.Drawing.Size(21, 14);
		this.label46.TabIndex = 229;
		this.label46.Text = "mA";
		this.textBoxZeroCurrZ.Location = new System.Drawing.Point(242, 134);
		this.textBoxZeroCurrZ.Name = "textBoxZeroCurrZ";
		this.textBoxZeroCurrZ.ReadOnly = true;
		this.textBoxZeroCurrZ.Size = new System.Drawing.Size(75, 23);
		this.textBoxZeroCurrZ.TabIndex = 208;
		this.label38.AutoSize = true;
		this.label38.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.label38.Location = new System.Drawing.Point(856, 190);
		this.label38.Name = "label38";
		this.label38.Size = new System.Drawing.Size(37, 14);
		this.label38.TabIndex = 199;
		this.label38.Text = "输出";
		this.cToggleButtonOutputZ.AutoSize = true;
		this.cToggleButtonOutputZ.Enabled = false;
		this.cToggleButtonOutputZ.Location = new System.Drawing.Point(905, 186);
		this.cToggleButtonOutputZ.MinimumSize = new System.Drawing.Size(45, 22);
		this.cToggleButtonOutputZ.Name = "cToggleButtonOutputZ";
		this.cToggleButtonOutputZ.OffBackColor = System.Drawing.Color.Gray;
		this.cToggleButtonOutputZ.OffToggleColor = System.Drawing.Color.Gainsboro;
		this.cToggleButtonOutputZ.OnBackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		this.cToggleButtonOutputZ.OnToggleColor = System.Drawing.Color.WhiteSmoke;
		this.cToggleButtonOutputZ.Size = new System.Drawing.Size(45, 22);
		this.cToggleButtonOutputZ.TabIndex = 198;
		this.cToggleButtonOutputZ.UseVisualStyleBackColor = true;
		this.cToggleButtonOutputZ.CheckedChanged += new System.EventHandler(cToggleButtonOutputZ_CheckedChanged);
		this.textBoxRecurMagZ.Location = new System.Drawing.Point(866, 134);
		this.textBoxRecurMagZ.Name = "textBoxRecurMagZ";
		this.textBoxRecurMagZ.ReadOnly = true;
		this.textBoxRecurMagZ.Size = new System.Drawing.Size(84, 23);
		this.textBoxRecurMagZ.TabIndex = 157;
		this.label40.AutoSize = true;
		this.label40.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.label40.Location = new System.Drawing.Point(742, 138);
		this.label40.Name = "label40";
		this.label40.Size = new System.Drawing.Size(119, 14);
		this.label40.TabIndex = 156;
		this.label40.Text = "复现磁场计算值：";
		this.label41.AutoSize = true;
		this.label41.Location = new System.Drawing.Point(957, 138);
		this.label41.Name = "label41";
		this.label41.Size = new System.Drawing.Size(21, 14);
		this.label41.TabIndex = 158;
		this.label41.Text = "nT";
		this.label42.AutoSize = true;
		this.label42.Location = new System.Drawing.Point(320, 172);
		this.label42.Name = "label42";
		this.label42.Size = new System.Drawing.Size(21, 14);
		this.label42.TabIndex = 155;
		this.label42.Text = "mA";
		this.label43.AutoSize = true;
		this.label43.Location = new System.Drawing.Point(91, 171);
		this.label43.Name = "label43";
		this.label43.Size = new System.Drawing.Size(133, 14);
		this.label43.TabIndex = 154;
		this.label43.Text = "输出总电流回读值：";
		this.textBoxTotalCurrZ.Location = new System.Drawing.Point(242, 168);
		this.textBoxTotalCurrZ.Name = "textBoxTotalCurrZ";
		this.textBoxTotalCurrZ.ReadOnly = true;
		this.textBoxTotalCurrZ.Size = new System.Drawing.Size(75, 23);
		this.textBoxTotalCurrZ.TabIndex = 153;
		this.label44.Font = new System.Drawing.Font("Microsoft Sans Serif", 18f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label44.ForeColor = System.Drawing.SystemColors.Highlight;
		this.label44.Location = new System.Drawing.Point(28, 68);
		this.label44.Name = "label44";
		this.label44.Size = new System.Drawing.Size(45, 92);
		this.label44.TabIndex = 9;
		this.label44.Text = "Z轴";
		this.timer1.Tick += new System.EventHandler(timer1_Tick);
		this.timer2.Interval = 500;
		this.timer2.Tick += new System.EventHandler(timer2_Tick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 14f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1085, 767);
		base.Controls.Add(this.panel3);
		base.Controls.Add(this.panel2);
		base.Controls.Add(this.panel1);
		base.Controls.Add(this.toolStrip1);
		this.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Margin = new System.Windows.Forms.Padding(4);
		base.MaximizeBox = false;
		base.Name = "FormMain";
		this.Text = "磁场控制系统软件";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FormMain_FormClosing);
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(FormMain_FormClosed);
		base.Load += new System.EventHandler(FormMain_Load);
		this.toolStrip1.ResumeLayout(false);
		this.toolStrip1.PerformLayout();
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		this.panel2.ResumeLayout(false);
		this.panel2.PerformLayout();
		this.panel3.ResumeLayout(false);
		this.panel3.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
