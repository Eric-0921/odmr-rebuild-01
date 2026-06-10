using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CControls;

public class CRegulator6x1 : UserControl
{
	public int maxValueInt = 1000000;

	public int minValueInt;

	private IContainer components;

	private Button buttonMinus7;

	private Button buttonAdd7;

	private Button buttonMinus6;

	private Button buttonAdd6;

	private SevenSegmentArray sevenSegmentArray1;

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

	[Category("Custom")]
	public double MaxValue
	{
		get
		{
			return (double)maxValueInt / 10.0;
		}
		set
		{
			if (value * 10.0 > (double)minValueInt)
			{
				maxValueInt = (int)(value * 10.0);
			}
		}
	}

	[Category("Custom")]
	public double MinValue
	{
		get
		{
			return minValueInt / 10;
		}
		set
		{
			if (value * 10.0 < (double)maxValueInt)
			{
				minValueInt = (int)(value * 10.0);
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
			sevenSegmentArray1.Value = $"{value:000000.0}";
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

	public CRegulator6x1()
	{
		InitializeComponent();
		sevenSegmentArray1.Value = "000000.0";
	}

	private void SetValue(double val)
	{
		sevenSegmentArray1.Value = $"{val:000000.0}";
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
		num += 1000000;
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
		num += 100000;
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
		num += 10000;
		if (num > maxValueInt)
		{
			num = maxValueInt;
		}
		double value = (double)num * 0.1;
		SetValue(value);
	}

	private void buttonAdd4_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 10.0);
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
		double value = (double)num * 0.1;
		SetValue(value);
	}

	private void buttonAdd5_Click(object sender, EventArgs e)
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

	private void buttonAdd6_Click(object sender, EventArgs e)
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

	private void buttonAdd7_Click(object sender, EventArgs e)
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
		num -= 1000000;
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
		num -= 100000;
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
		num -= 10000;
		if (num < minValueInt)
		{
			num = minValueInt;
		}
		double value = (double)num * 0.1;
		SetValue(value);
	}

	private void buttonMinus4_Click(object sender, EventArgs e)
	{
		int num = (int)Math.Round(GetValue() * 10.0);
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
		double value = (double)num * 0.1;
		SetValue(value);
	}

	private void buttonMinus5_Click(object sender, EventArgs e)
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

	private void buttonMinus6_Click(object sender, EventArgs e)
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

	private void buttonMinus7_Click(object sender, EventArgs e)
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

	private void CRegulator6x1_Resize(object sender, EventArgs e)
	{
		base.Size = new Size(195, 97);
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
		this.buttonMinus7 = new System.Windows.Forms.Button();
		this.buttonAdd7 = new System.Windows.Forms.Button();
		this.buttonMinus6 = new System.Windows.Forms.Button();
		this.buttonAdd6 = new System.Windows.Forms.Button();
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
		this.sevenSegmentArray1 = new CControls.SevenSegmentArray();
		base.SuspendLayout();
		this.buttonMinus7.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonMinus7.Location = new System.Drawing.Point(166, 70);
		this.buttonMinus7.Name = "buttonMinus7";
		this.buttonMinus7.Size = new System.Drawing.Size(25, 25);
		this.buttonMinus7.TabIndex = 214;
		this.buttonMinus7.Text = "-";
		this.buttonMinus7.UseVisualStyleBackColor = true;
		this.buttonMinus7.Click += new System.EventHandler(buttonMinus7_Click);
		this.buttonAdd7.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonAdd7.Location = new System.Drawing.Point(166, 0);
		this.buttonAdd7.Name = "buttonAdd7";
		this.buttonAdd7.Size = new System.Drawing.Size(25, 25);
		this.buttonAdd7.TabIndex = 213;
		this.buttonAdd7.Text = "+";
		this.buttonAdd7.UseVisualStyleBackColor = true;
		this.buttonAdd7.Click += new System.EventHandler(buttonAdd7_Click);
		this.buttonMinus6.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonMinus6.Location = new System.Drawing.Point(139, 70);
		this.buttonMinus6.Name = "buttonMinus6";
		this.buttonMinus6.Size = new System.Drawing.Size(25, 25);
		this.buttonMinus6.TabIndex = 212;
		this.buttonMinus6.Text = "-";
		this.buttonMinus6.UseVisualStyleBackColor = true;
		this.buttonMinus6.Click += new System.EventHandler(buttonMinus6_Click);
		this.buttonAdd6.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonAdd6.Location = new System.Drawing.Point(139, 0);
		this.buttonAdd6.Name = "buttonAdd6";
		this.buttonAdd6.Size = new System.Drawing.Size(25, 25);
		this.buttonAdd6.TabIndex = 211;
		this.buttonAdd6.Text = "+";
		this.buttonAdd6.UseVisualStyleBackColor = true;
		this.buttonAdd6.Click += new System.EventHandler(buttonAdd6_Click);
		this.buttonAdd3.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonAdd3.Location = new System.Drawing.Point(59, 0);
		this.buttonAdd3.Name = "buttonAdd3";
		this.buttonAdd3.Size = new System.Drawing.Size(25, 25);
		this.buttonAdd3.TabIndex = 204;
		this.buttonAdd3.Text = "+";
		this.buttonAdd3.UseVisualStyleBackColor = true;
		this.buttonAdd3.Click += new System.EventHandler(buttonAdd3_Click);
		this.buttonAdd4.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonAdd4.Location = new System.Drawing.Point(86, 0);
		this.buttonAdd4.Name = "buttonAdd4";
		this.buttonAdd4.Size = new System.Drawing.Size(25, 25);
		this.buttonAdd4.TabIndex = 206;
		this.buttonAdd4.Text = "+";
		this.buttonAdd4.UseVisualStyleBackColor = true;
		this.buttonAdd4.Click += new System.EventHandler(buttonAdd4_Click);
		this.buttonMinus3.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonMinus3.Location = new System.Drawing.Point(59, 70);
		this.buttonMinus3.Name = "buttonMinus3";
		this.buttonMinus3.Size = new System.Drawing.Size(25, 25);
		this.buttonMinus3.TabIndex = 205;
		this.buttonMinus3.Text = "-";
		this.buttonMinus3.UseVisualStyleBackColor = true;
		this.buttonMinus3.Click += new System.EventHandler(buttonMinus3_Click);
		this.buttonMinus4.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonMinus4.Location = new System.Drawing.Point(86, 70);
		this.buttonMinus4.Name = "buttonMinus4";
		this.buttonMinus4.Size = new System.Drawing.Size(25, 25);
		this.buttonMinus4.TabIndex = 207;
		this.buttonMinus4.Text = "-";
		this.buttonMinus4.UseVisualStyleBackColor = true;
		this.buttonMinus4.Click += new System.EventHandler(buttonMinus4_Click);
		this.buttonAdd5.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonAdd5.Location = new System.Drawing.Point(113, 0);
		this.buttonAdd5.Name = "buttonAdd5";
		this.buttonAdd5.Size = new System.Drawing.Size(25, 25);
		this.buttonAdd5.TabIndex = 208;
		this.buttonAdd5.Text = "+";
		this.buttonAdd5.UseVisualStyleBackColor = true;
		this.buttonAdd5.Click += new System.EventHandler(buttonAdd5_Click);
		this.buttonMinus2.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonMinus2.Location = new System.Drawing.Point(31, 70);
		this.buttonMinus2.Name = "buttonMinus2";
		this.buttonMinus2.Size = new System.Drawing.Size(25, 25);
		this.buttonMinus2.TabIndex = 203;
		this.buttonMinus2.Text = "-";
		this.buttonMinus2.UseVisualStyleBackColor = true;
		this.buttonMinus2.Click += new System.EventHandler(buttonMinus2_Click);
		this.buttonMinus5.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonMinus5.Location = new System.Drawing.Point(113, 70);
		this.buttonMinus5.Name = "buttonMinus5";
		this.buttonMinus5.Size = new System.Drawing.Size(25, 25);
		this.buttonMinus5.TabIndex = 209;
		this.buttonMinus5.Text = "-";
		this.buttonMinus5.UseVisualStyleBackColor = true;
		this.buttonMinus5.Click += new System.EventHandler(buttonMinus5_Click);
		this.buttonAdd2.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonAdd2.Location = new System.Drawing.Point(31, 0);
		this.buttonAdd2.Name = "buttonAdd2";
		this.buttonAdd2.Size = new System.Drawing.Size(25, 25);
		this.buttonAdd2.TabIndex = 202;
		this.buttonAdd2.Text = "+";
		this.buttonAdd2.UseVisualStyleBackColor = true;
		this.buttonAdd2.Click += new System.EventHandler(buttonAdd2_Click);
		this.buttonMinus1.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonMinus1.Location = new System.Drawing.Point(3, 70);
		this.buttonMinus1.Name = "buttonMinus1";
		this.buttonMinus1.Size = new System.Drawing.Size(25, 25);
		this.buttonMinus1.TabIndex = 201;
		this.buttonMinus1.Text = "-";
		this.buttonMinus1.UseVisualStyleBackColor = true;
		this.buttonMinus1.Click += new System.EventHandler(buttonMinus1_Click);
		this.buttonAdd1.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.buttonAdd1.Location = new System.Drawing.Point(3, 0);
		this.buttonAdd1.Name = "buttonAdd1";
		this.buttonAdd1.Size = new System.Drawing.Size(25, 25);
		this.buttonAdd1.TabIndex = 200;
		this.buttonAdd1.Text = "+";
		this.buttonAdd1.UseVisualStyleBackColor = true;
		this.buttonAdd1.Click += new System.EventHandler(buttonAdd1_Click);
		this.sevenSegmentArray1.ArrayCount = 7;
		this.sevenSegmentArray1.ColorBackground = System.Drawing.Color.Black;
		this.sevenSegmentArray1.ColorDark = System.Drawing.Color.FromArgb(80, 0, 0);
		this.sevenSegmentArray1.ColorLight = System.Drawing.Color.Red;
		this.sevenSegmentArray1.DecimalShow = true;
		this.sevenSegmentArray1.ElementPadding = new System.Windows.Forms.Padding(8, 4, 4, 4);
		this.sevenSegmentArray1.ElementWidth = 10;
		this.sevenSegmentArray1.ItalicFactor = -0.1f;
		this.sevenSegmentArray1.Location = new System.Drawing.Point(3, 27);
		this.sevenSegmentArray1.Name = "sevenSegmentArray1";
		this.sevenSegmentArray1.Size = new System.Drawing.Size(189, 41);
		this.sevenSegmentArray1.TabIndex = 210;
		this.sevenSegmentArray1.TabStop = false;
		this.sevenSegmentArray1.Value = "110220.3";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 14f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.buttonMinus7);
		base.Controls.Add(this.buttonAdd7);
		base.Controls.Add(this.buttonMinus6);
		base.Controls.Add(this.buttonAdd6);
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
		base.Name = "CRegulator6x1";
		base.Size = new System.Drawing.Size(195, 97);
		base.Resize += new System.EventHandler(CRegulator6x1_Resize);
		base.ResumeLayout(false);
	}
}
