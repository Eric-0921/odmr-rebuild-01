using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SimplePowerController;

public class FormParamSet : Form
{
	private IContainer components;

	private GroupBox groupBox2;

	private Label label7;

	private TextBox textBoxConstantZ;

	private Label label6;

	private TextBox textBoxConstantY;

	private Label label5;

	private TextBox textBoxConstantX;

	private Button buttonCancel;

	private Button buttonOK;

	private Label label3;

	private Label label2;

	private Label label1;

	public FormParamSet()
	{
		InitializeComponent();
	}

	private void FormParamSet_Load(object sender, EventArgs e)
	{
		LocalSettingAccessor.ReadCoilConstant(out var coilConstantX, out var coilConstantY, out var coilConstantZ);
		textBoxConstantX.Text = coilConstantX.ToString();
		textBoxConstantY.Text = coilConstantY.ToString();
		textBoxConstantZ.Text = coilConstantZ.ToString();
	}

	private void buttonOK_Click(object sender, EventArgs e)
	{
		double result;
		bool num = double.TryParse(textBoxConstantX.Text, out result);
		double result2;
		bool flag = double.TryParse(textBoxConstantY.Text, out result2);
		double result3;
		bool flag2 = double.TryParse(textBoxConstantZ.Text, out result3);
		if (!num || !flag || !flag2)
		{
			MessageBox.Show("线圈常数输入不合法！");
			return;
		}
		if (result == 0.0 || result2 == 0.0 || result3 == 0.0)
		{
			MessageBox.Show("线圈常数不能为0！");
			return;
		}
		LocalSettingAccessor.SaveCoilConstant(result, result2, result3);
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
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.label7 = new System.Windows.Forms.Label();
		this.textBoxConstantZ = new System.Windows.Forms.TextBox();
		this.label6 = new System.Windows.Forms.Label();
		this.textBoxConstantY = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.textBoxConstantX = new System.Windows.Forms.TextBox();
		this.buttonCancel = new System.Windows.Forms.Button();
		this.buttonOK = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.groupBox2.SuspendLayout();
		base.SuspendLayout();
		this.groupBox2.Controls.Add(this.label3);
		this.groupBox2.Controls.Add(this.label2);
		this.groupBox2.Controls.Add(this.label1);
		this.groupBox2.Controls.Add(this.label7);
		this.groupBox2.Controls.Add(this.textBoxConstantZ);
		this.groupBox2.Controls.Add(this.label6);
		this.groupBox2.Controls.Add(this.textBoxConstantY);
		this.groupBox2.Controls.Add(this.label5);
		this.groupBox2.Controls.Add(this.textBoxConstantX);
		this.groupBox2.Location = new System.Drawing.Point(12, 60);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(590, 91);
		this.groupBox2.TabIndex = 57;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "线圈常数";
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(398, 40);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(42, 14);
		this.label7.TabIndex = 5;
		this.label7.Text = "Z轴：";
		this.textBoxConstantZ.Location = new System.Drawing.Point(452, 36);
		this.textBoxConstantZ.Name = "textBoxConstantZ";
		this.textBoxConstantZ.Size = new System.Drawing.Size(83, 23);
		this.textBoxConstantZ.TabIndex = 4;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(207, 40);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(42, 14);
		this.label6.TabIndex = 3;
		this.label6.Text = "Y轴：";
		this.textBoxConstantY.Location = new System.Drawing.Point(258, 36);
		this.textBoxConstantY.Name = "textBoxConstantY";
		this.textBoxConstantY.Size = new System.Drawing.Size(83, 23);
		this.textBoxConstantY.TabIndex = 2;
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(15, 40);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(42, 14);
		this.label5.TabIndex = 1;
		this.label5.Text = "X轴：";
		this.textBoxConstantX.Location = new System.Drawing.Point(67, 36);
		this.textBoxConstantX.Name = "textBoxConstantX";
		this.textBoxConstantX.Size = new System.Drawing.Size(83, 23);
		this.textBoxConstantX.TabIndex = 0;
		this.buttonCancel.Location = new System.Drawing.Point(448, 226);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(79, 28);
		this.buttonCancel.TabIndex = 59;
		this.buttonCancel.Text = "取消";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.buttonCancel.Click += new System.EventHandler(buttonCancel_Click);
		this.buttonOK.Location = new System.Drawing.Point(354, 226);
		this.buttonOK.Name = "buttonOK";
		this.buttonOK.Size = new System.Drawing.Size(79, 28);
		this.buttonOK.TabIndex = 58;
		this.buttonOK.Text = "确认";
		this.buttonOK.UseVisualStyleBackColor = true;
		this.buttonOK.Click += new System.EventHandler(buttonOK_Click);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(153, 40);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(42, 14);
		this.label1.TabIndex = 6;
		this.label1.Text = "nT/mA";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(339, 40);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(42, 14);
		this.label2.TabIndex = 60;
		this.label2.Text = "nT/mA";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(538, 40);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(42, 14);
		this.label3.TabIndex = 61;
		this.label3.Text = "nT/mA";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 14f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(614, 289);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.buttonOK);
		base.Controls.Add(this.groupBox2);
		this.Font = new System.Drawing.Font("宋体", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Margin = new System.Windows.Forms.Padding(4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FormParamSet";
		this.Text = "参数设置";
		base.Load += new System.EventHandler(FormParamSet_Load);
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		base.ResumeLayout(false);
	}
}
