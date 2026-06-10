using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CControls;

public class CRegulator2x1 : UserControl
{
	public int maxValueInt = 650;

	public int minValueInt = 450;

	private IContainer components;

	private Button buttonAdd1;

	private Button buttonAdd2;

	private Button buttonMinus1;

	private Button buttonMinus2;

	private Button buttonAdd3;

	private Button buttonMinus3;

	private SevenSegmentArray sevenSegmentArray1;

	[Category("Custom")]
	public int MaxValue
	{
		get
		{
			return maxValueInt / 10;
		}
		set
		{
			if (value * 10 > minValueInt)
			{
				maxValueInt = value * 10;
			}
		}
	}

	[Category("Custom")]
	public int MinValue
	{
		get
		{
			return minValueInt / 10;
		}
		set
		{
			if (value * 10 < maxValueInt)
			{
				minValueInt = value * 10;
			}
		}
	}

	[Category("Custom")]
	public double Value
	{
		get
		{
			return GetValue();
		}
		set
		{
			sevenSegmentArray1.Value = $"{value:00.0}";
		}
	}

	[Category("Custom")]
	public Color ColorBackground
	{
		get
		{
			return sevenSegmentArray1.ColorBackground;
		}
		set
		{
			sevenSegmentArray1.ColorBackground = value;
		}
	}

	[Category("Custom")]
	public Color ColorDark
	{
		get
		{
			return sevenSegmentArray1.ColorDark;
		}
		set
		{
			sevenSegmentArray1.ColorDark = value;
		}
	}

	[Category("Custom")]
	public Color ColorLight
	{
		get
		{
			return sevenSegmentArray1.ColorLight;
		}
		set
		{
			sevenSegmentArray1.ColorLight = value;
		}
	}

	public event EventHandler<double> ValueChanged;

	public CRegulator2x1()
	{
		InitializeComponent();
		sevenSegmentArray1.Value = "50.0";
	}

	private void SetValue(double val)
	{
		sevenSegmentArray1.Value = $"{val:00.0}";
		if (this.ValueChanged != null)
		{
			this.ValueChanged(this, val);
		}
	}

	private double GetValue()
	{
		return double.Parse(sevenSegmentArray1.Value);
	}

	private void buttonAdd1_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 10.0);
		if (num >= maxValueInt)
		{
			MessageBox.Show("已到最大值！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		num += 100;
		if (num > maxValueInt)
		{
			num = maxValueInt;
		}
		double value = (double)num * 0.1;
		SetValue(value);
	}

	private void buttonAdd2_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 10.0);
		if (num >= maxValueInt)
		{
			MessageBox.Show("已到最大值！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		num += 10;
		if (num > maxValueInt)
		{
			num = maxValueInt;
		}
		double value = (double)num * 0.1;
		SetValue(value);
	}

	private void buttonAdd3_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 10.0);
		if (num >= maxValueInt)
		{
			MessageBox.Show("已到最大值！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		num++;
		if (num > maxValueInt)
		{
			num = maxValueInt;
		}
		double value = (double)num * 0.1;
		SetValue(value);
	}

	private void buttonMinus1_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 10.0);
		if (num <= minValueInt)
		{
			MessageBox.Show("已到最小值！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		num -= 100;
		if (num < minValueInt)
		{
			num = minValueInt;
		}
		double value = (double)num * 0.1;
		SetValue(value);
	}

	private void buttonMinus2_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 10.0);
		if (num <= minValueInt)
		{
			MessageBox.Show("已到最小值！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		num -= 10;
		if (num < minValueInt)
		{
			num = minValueInt;
		}
		double value = (double)num * 0.1;
		SetValue(value);
	}

	private void buttonMinus3_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 10.0);
		if (num <= minValueInt)
		{
			MessageBox.Show("已到最小值！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		num--;
		if (num < minValueInt)
		{
			num = minValueInt;
		}
		double value = (double)num * 0.1;
		SetValue(value);
	}

	private void CRegulator2x1_Resize(object sender, EventArgs e)
	{
		base.Size = new Size(126, 117);
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
		this.buttonAdd1 = new System.Windows.Forms.Button();
		this.buttonAdd2 = new System.Windows.Forms.Button();
		this.buttonMinus1 = new System.Windows.Forms.Button();
		this.buttonMinus2 = new System.Windows.Forms.Button();
		this.buttonAdd3 = new System.Windows.Forms.Button();
		this.buttonMinus3 = new System.Windows.Forms.Button();
		this.sevenSegmentArray1 = new CControls.SevenSegmentArray();
		base.SuspendLayout();
		this.buttonAdd1.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonAdd1.Location = new System.Drawing.Point(6, 3);
		this.buttonAdd1.Name = "buttonAdd1";
		this.buttonAdd1.Size = new System.Drawing.Size(33, 29);
		this.buttonAdd1.TabIndex = 184;
		this.buttonAdd1.Text = "+";
		this.buttonAdd1.UseVisualStyleBackColor = true;
		this.buttonAdd1.Click += new System.EventHandler(buttonAdd1_Click);
		this.buttonAdd2.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonAdd2.Location = new System.Drawing.Point(47, 3);
		this.buttonAdd2.Name = "buttonAdd2";
		this.buttonAdd2.Size = new System.Drawing.Size(33, 29);
		this.buttonAdd2.TabIndex = 187;
		this.buttonAdd2.Text = "+";
		this.buttonAdd2.UseVisualStyleBackColor = true;
		this.buttonAdd2.Click += new System.EventHandler(buttonAdd2_Click);
		this.buttonMinus1.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonMinus1.Location = new System.Drawing.Point(6, 86);
		this.buttonMinus1.Name = "buttonMinus1";
		this.buttonMinus1.Size = new System.Drawing.Size(33, 29);
		this.buttonMinus1.TabIndex = 186;
		this.buttonMinus1.Text = "-";
		this.buttonMinus1.UseVisualStyleBackColor = true;
		this.buttonMinus1.Click += new System.EventHandler(buttonMinus1_Click);
		this.buttonMinus2.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonMinus2.Location = new System.Drawing.Point(47, 86);
		this.buttonMinus2.Name = "buttonMinus2";
		this.buttonMinus2.Size = new System.Drawing.Size(33, 29);
		this.buttonMinus2.TabIndex = 189;
		this.buttonMinus2.Text = "-";
		this.buttonMinus2.UseVisualStyleBackColor = true;
		this.buttonMinus2.Click += new System.EventHandler(buttonMinus2_Click);
		this.buttonAdd3.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonAdd3.Location = new System.Drawing.Point(88, 3);
		this.buttonAdd3.Name = "buttonAdd3";
		this.buttonAdd3.Size = new System.Drawing.Size(33, 29);
		this.buttonAdd3.TabIndex = 190;
		this.buttonAdd3.Text = "+";
		this.buttonAdd3.UseVisualStyleBackColor = true;
		this.buttonAdd3.Click += new System.EventHandler(buttonAdd3_Click);
		this.buttonMinus3.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonMinus3.Location = new System.Drawing.Point(88, 86);
		this.buttonMinus3.Name = "buttonMinus3";
		this.buttonMinus3.Size = new System.Drawing.Size(33, 29);
		this.buttonMinus3.TabIndex = 192;
		this.buttonMinus3.Text = "-";
		this.buttonMinus3.UseVisualStyleBackColor = true;
		this.buttonMinus3.Click += new System.EventHandler(buttonMinus3_Click);
		this.sevenSegmentArray1.ArrayCount = 3;
		this.sevenSegmentArray1.ColorBackground = System.Drawing.Color.Black;
		this.sevenSegmentArray1.ColorDark = System.Drawing.Color.FromArgb(0, 64, 0);
		this.sevenSegmentArray1.ColorLight = System.Drawing.Color.Lime;
		this.sevenSegmentArray1.DecimalShow = true;
		this.sevenSegmentArray1.ElementPadding = new System.Windows.Forms.Padding(8, 4, 4, 4);
		this.sevenSegmentArray1.ElementWidth = 10;
		this.sevenSegmentArray1.ItalicFactor = -0.1f;
		this.sevenSegmentArray1.Location = new System.Drawing.Point(3, 34);
		this.sevenSegmentArray1.Name = "sevenSegmentArray1";
		this.sevenSegmentArray1.Size = new System.Drawing.Size(120, 50);
		this.sevenSegmentArray1.TabIndex = 194;
		this.sevenSegmentArray1.TabStop = false;
		this.sevenSegmentArray1.Value = "20.3";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 14f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.sevenSegmentArray1);
		base.Controls.Add(this.buttonAdd1);
		base.Controls.Add(this.buttonAdd2);
		base.Controls.Add(this.buttonMinus1);
		base.Controls.Add(this.buttonMinus2);
		base.Controls.Add(this.buttonAdd3);
		base.Controls.Add(this.buttonMinus3);
		this.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(4);
		base.Name = "CRegulator2x1";
		base.Size = new System.Drawing.Size(126, 117);
		base.Resize += new System.EventHandler(CRegulator2x1_Resize);
		base.ResumeLayout(false);
	}
}
