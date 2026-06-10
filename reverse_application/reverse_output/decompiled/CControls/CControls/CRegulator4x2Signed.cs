using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CControls;

public class CRegulator4x2Signed : UserControl
{
	public int maxValueInt = 500000;

	public int minValueInt;

	private IContainer components;

	private Button buttonAdd3;

	private Button buttonAdd4;

	private Button buttonMinus3;

	private Button buttonMinus4;

	private Button buttonAdd5;

	private Button buttonMinus2;

	private Button buttonMinus5;

	private Button buttonAdd2;

	private Button buttonMinus1;

	private Button buttonAdd1;

	private SevenSegmentArray sevenSegmentArray1;

	private Button buttonAdd6;

	private Button buttonMinus6;

	[Category("Custom")]
	public double MaxValue
	{
		get
		{
			return (double)maxValueInt / 100.0;
		}
		set
		{
			if (value * 100.0 > (double)minValueInt)
			{
				maxValueInt = (int)(value * 100.0);
			}
		}
	}

	[Category("Custom")]
	public double MinValue
	{
		get
		{
			return minValueInt / 100;
		}
		set
		{
			if (value * 100.0 < (double)maxValueInt)
			{
				minValueInt = (int)(value * 100.0);
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
			sevenSegmentArray1.Value = $"{value:0000.00}";
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

	public CRegulator4x2Signed()
	{
		InitializeComponent();
		sevenSegmentArray1.Value = "0000.00";
	}

	private void SetValue(double val)
	{
		sevenSegmentArray1.Value = $"{val:0000.00}";
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
		int num = (int)Math.Round(GetValue() * 100.0);
		if (num >= maxValueInt)
		{
			MessageBox.Show("已到最大值！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		num += 100000;
		if (num > maxValueInt)
		{
			num = maxValueInt;
		}
		double value = (double)num * 0.01;
		SetValue(value);
	}

	private void buttonAdd2_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 100.0);
		if (num >= maxValueInt)
		{
			MessageBox.Show("已到最大值！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		num += 10000;
		if (num > maxValueInt)
		{
			num = maxValueInt;
		}
		double value = (double)num * 0.01;
		SetValue(value);
	}

	private void buttonAdd3_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 100.0);
		if (num >= maxValueInt)
		{
			MessageBox.Show("已到最大值！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		num += 1000;
		if (num > maxValueInt)
		{
			num = maxValueInt;
		}
		double value = (double)num * 0.01;
		SetValue(value);
	}

	private void buttonAdd4_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 100.0);
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
		double value = (double)num * 0.01;
		SetValue(value);
	}

	private void buttonAdd5_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 100.0);
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
		double value = (double)num * 0.01;
		SetValue(value);
	}

	private void buttonAdd6_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 100.0);
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
		double value = (double)num * 0.01;
		SetValue(value);
	}

	private void buttonMinus1_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 100.0);
		if (num <= minValueInt)
		{
			MessageBox.Show("已到最小值！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		num -= 100000;
		if (num < minValueInt)
		{
			num = minValueInt;
		}
		double value = (double)num * 0.01;
		SetValue(value);
	}

	private void buttonMinus2_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 100.0);
		if (num <= minValueInt)
		{
			MessageBox.Show("已到最小值！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		num -= 10000;
		if (num < minValueInt)
		{
			num = minValueInt;
		}
		double value = (double)num * 0.01;
		SetValue(value);
	}

	private void buttonMinus3_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 100.0);
		if (num <= minValueInt)
		{
			MessageBox.Show("已到最小值！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		num -= 1000;
		if (num < minValueInt)
		{
			num = minValueInt;
		}
		double value = (double)num * 0.01;
		SetValue(value);
	}

	private void buttonMinus4_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 100.0);
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
		double value = (double)num * 0.01;
		SetValue(value);
	}

	private void buttonMinus5_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 100.0);
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
		double value = (double)num * 0.01;
		SetValue(value);
	}

	private void buttonMinus6_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 100.0);
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
		double value = (double)num * 0.01;
		SetValue(value);
	}

	private void CRegulator4x2_Resize(object sender, EventArgs e)
	{
		base.Size = new Size(196, 98);
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
		this.buttonAdd3 = new System.Windows.Forms.Button();
		this.buttonAdd4 = new System.Windows.Forms.Button();
		this.buttonMinus3 = new System.Windows.Forms.Button();
		this.buttonMinus4 = new System.Windows.Forms.Button();
		this.buttonAdd5 = new System.Windows.Forms.Button();
		this.buttonMinus2 = new System.Windows.Forms.Button();
		this.buttonMinus5 = new System.Windows.Forms.Button();
		this.buttonAdd2 = new System.Windows.Forms.Button();
		this.buttonMinus1 = new System.Windows.Forms.Button();
		this.buttonAdd1 = new System.Windows.Forms.Button();
		this.buttonAdd6 = new System.Windows.Forms.Button();
		this.buttonMinus6 = new System.Windows.Forms.Button();
		this.sevenSegmentArray1 = new CControls.SevenSegmentArray();
		base.SuspendLayout();
		this.buttonAdd3.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonAdd3.Location = new System.Drawing.Point(86, 1);
		this.buttonAdd3.Name = "buttonAdd3";
		this.buttonAdd3.Size = new System.Drawing.Size(25, 25);
		this.buttonAdd3.TabIndex = 158;
		this.buttonAdd3.Text = "+";
		this.buttonAdd3.UseVisualStyleBackColor = true;
		this.buttonAdd3.Click += new System.EventHandler(buttonAdd3_Click);
		this.buttonAdd4.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonAdd4.Location = new System.Drawing.Point(113, 1);
		this.buttonAdd4.Name = "buttonAdd4";
		this.buttonAdd4.Size = new System.Drawing.Size(25, 25);
		this.buttonAdd4.TabIndex = 161;
		this.buttonAdd4.Text = "+";
		this.buttonAdd4.UseVisualStyleBackColor = true;
		this.buttonAdd4.Click += new System.EventHandler(buttonAdd4_Click);
		this.buttonMinus3.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonMinus3.Location = new System.Drawing.Point(86, 71);
		this.buttonMinus3.Name = "buttonMinus3";
		this.buttonMinus3.Size = new System.Drawing.Size(25, 25);
		this.buttonMinus3.TabIndex = 160;
		this.buttonMinus3.Text = "-";
		this.buttonMinus3.UseVisualStyleBackColor = true;
		this.buttonMinus3.Click += new System.EventHandler(buttonMinus3_Click);
		this.buttonMinus4.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonMinus4.Location = new System.Drawing.Point(113, 71);
		this.buttonMinus4.Name = "buttonMinus4";
		this.buttonMinus4.Size = new System.Drawing.Size(25, 25);
		this.buttonMinus4.TabIndex = 163;
		this.buttonMinus4.Text = "-";
		this.buttonMinus4.UseVisualStyleBackColor = true;
		this.buttonMinus4.Click += new System.EventHandler(buttonMinus4_Click);
		this.buttonAdd5.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonAdd5.Location = new System.Drawing.Point(140, 0);
		this.buttonAdd5.Name = "buttonAdd5";
		this.buttonAdd5.Size = new System.Drawing.Size(25, 25);
		this.buttonAdd5.TabIndex = 164;
		this.buttonAdd5.Text = "+";
		this.buttonAdd5.UseVisualStyleBackColor = true;
		this.buttonAdd5.Click += new System.EventHandler(buttonAdd5_Click);
		this.buttonMinus2.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonMinus2.Location = new System.Drawing.Point(59, 71);
		this.buttonMinus2.Name = "buttonMinus2";
		this.buttonMinus2.Size = new System.Drawing.Size(25, 25);
		this.buttonMinus2.TabIndex = 157;
		this.buttonMinus2.Text = "-";
		this.buttonMinus2.UseVisualStyleBackColor = true;
		this.buttonMinus2.Click += new System.EventHandler(buttonMinus2_Click);
		this.buttonMinus5.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonMinus5.Location = new System.Drawing.Point(140, 71);
		this.buttonMinus5.Name = "buttonMinus5";
		this.buttonMinus5.Size = new System.Drawing.Size(25, 25);
		this.buttonMinus5.TabIndex = 166;
		this.buttonMinus5.Text = "-";
		this.buttonMinus5.UseVisualStyleBackColor = true;
		this.buttonMinus5.Click += new System.EventHandler(buttonMinus5_Click);
		this.buttonAdd2.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonAdd2.Location = new System.Drawing.Point(59, 1);
		this.buttonAdd2.Name = "buttonAdd2";
		this.buttonAdd2.Size = new System.Drawing.Size(25, 25);
		this.buttonAdd2.TabIndex = 155;
		this.buttonAdd2.Text = "+";
		this.buttonAdd2.UseVisualStyleBackColor = true;
		this.buttonAdd2.Click += new System.EventHandler(buttonAdd2_Click);
		this.buttonMinus1.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonMinus1.Location = new System.Drawing.Point(32, 71);
		this.buttonMinus1.Name = "buttonMinus1";
		this.buttonMinus1.Size = new System.Drawing.Size(25, 25);
		this.buttonMinus1.TabIndex = 154;
		this.buttonMinus1.Text = "-";
		this.buttonMinus1.UseVisualStyleBackColor = true;
		this.buttonMinus1.Click += new System.EventHandler(buttonMinus1_Click);
		this.buttonAdd1.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonAdd1.Location = new System.Drawing.Point(32, 1);
		this.buttonAdd1.Name = "buttonAdd1";
		this.buttonAdd1.Size = new System.Drawing.Size(25, 25);
		this.buttonAdd1.TabIndex = 152;
		this.buttonAdd1.Text = "+";
		this.buttonAdd1.UseVisualStyleBackColor = true;
		this.buttonAdd1.Click += new System.EventHandler(buttonAdd1_Click);
		this.buttonAdd6.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonAdd6.Location = new System.Drawing.Point(167, 0);
		this.buttonAdd6.Name = "buttonAdd6";
		this.buttonAdd6.Size = new System.Drawing.Size(25, 25);
		this.buttonAdd6.TabIndex = 185;
		this.buttonAdd6.Text = "+";
		this.buttonAdd6.UseVisualStyleBackColor = true;
		this.buttonAdd6.Click += new System.EventHandler(buttonAdd6_Click);
		this.buttonMinus6.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonMinus6.Location = new System.Drawing.Point(167, 71);
		this.buttonMinus6.Name = "buttonMinus6";
		this.buttonMinus6.Size = new System.Drawing.Size(25, 25);
		this.buttonMinus6.TabIndex = 186;
		this.buttonMinus6.Text = "-";
		this.buttonMinus6.UseVisualStyleBackColor = true;
		this.buttonMinus6.Click += new System.EventHandler(buttonMinus6_Click);
		this.sevenSegmentArray1.ArrayCount = 7;
		this.sevenSegmentArray1.ColorBackground = System.Drawing.Color.Black;
		this.sevenSegmentArray1.ColorDark = System.Drawing.Color.FromArgb(80, 0, 0);
		this.sevenSegmentArray1.ColorLight = System.Drawing.Color.Red;
		this.sevenSegmentArray1.DecimalShow = true;
		this.sevenSegmentArray1.ElementPadding = new System.Windows.Forms.Padding(8, 4, 4, 4);
		this.sevenSegmentArray1.ElementWidth = 10;
		this.sevenSegmentArray1.ItalicFactor = -0.1f;
		this.sevenSegmentArray1.Location = new System.Drawing.Point(3, 28);
		this.sevenSegmentArray1.Name = "sevenSegmentArray1";
		this.sevenSegmentArray1.Size = new System.Drawing.Size(189, 41);
		this.sevenSegmentArray1.TabIndex = 184;
		this.sevenSegmentArray1.TabStop = false;
		this.sevenSegmentArray1.Value = "0220.33";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 14f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.buttonAdd6);
		base.Controls.Add(this.buttonMinus6);
		base.Controls.Add(this.sevenSegmentArray1);
		base.Controls.Add(this.buttonAdd3);
		base.Controls.Add(this.buttonAdd4);
		base.Controls.Add(this.buttonMinus3);
		base.Controls.Add(this.buttonMinus4);
		base.Controls.Add(this.buttonAdd5);
		base.Controls.Add(this.buttonMinus2);
		base.Controls.Add(this.buttonMinus5);
		base.Controls.Add(this.buttonAdd2);
		base.Controls.Add(this.buttonMinus1);
		base.Controls.Add(this.buttonAdd1);
		this.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(4);
		base.Name = "CRegulator4x2Signed";
		base.Size = new System.Drawing.Size(196, 98);
		base.Resize += new System.EventHandler(CRegulator4x2_Resize);
		base.ResumeLayout(false);
	}
}
