using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SimplePowerController;

internal class AboutBox1 : Form
{
	private IContainer components;

	private PictureBox logoPictureBox;

	private Label labelProductName;

	private Label labelVersion;

	private Label labelCopyright;

	private Label labelCompanyName;

	private Button button1;

	private TextBox textBoxDescription;

	public string AssemblyTitle
	{
		get
		{
			object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				AssemblyTitleAttribute assemblyTitleAttribute = (AssemblyTitleAttribute)customAttributes[0];
				if (assemblyTitleAttribute.Title != "")
				{
					return assemblyTitleAttribute.Title;
				}
			}
			return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
		}
	}

	public string AssemblyVersion
	{
		get
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			return $"V{version.Major}.{version.Minor}.{version.Build}";
		}
	}

	public string AssemblyDescription
	{
		get
		{
			object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), inherit: false);
			if (customAttributes.Length == 0)
			{
				return "";
			}
			return ((AssemblyDescriptionAttribute)customAttributes[0]).Description;
		}
	}

	public string AssemblyProduct
	{
		get
		{
			object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), inherit: false);
			if (customAttributes.Length == 0)
			{
				return "";
			}
			return ((AssemblyProductAttribute)customAttributes[0]).Product;
		}
	}

	public string AssemblyCopyright
	{
		get
		{
			object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), inherit: false);
			if (customAttributes.Length == 0)
			{
				return "";
			}
			return ((AssemblyCopyrightAttribute)customAttributes[0]).Copyright;
		}
	}

	public string AssemblyCompany
	{
		get
		{
			object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), inherit: false);
			if (customAttributes.Length == 0)
			{
				return "";
			}
			return ((AssemblyCompanyAttribute)customAttributes[0]).Company;
		}
	}

	public AboutBox1()
	{
		InitializeComponent();
		Text = $"关于 {AssemblyTitle}";
		labelProductName.Text = AssemblyProduct;
		labelVersion.Text = $"版本 {AssemblyVersion}";
		labelCopyright.Text = AssemblyCopyright;
		labelCompanyName.Text = AssemblyCompany;
		textBoxDescription.Text = AssemblyDescription;
	}

	private void button1_Click(object sender, EventArgs e)
	{
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SimplePowerController.AboutBox1));
		this.labelProductName = new System.Windows.Forms.Label();
		this.labelVersion = new System.Windows.Forms.Label();
		this.labelCopyright = new System.Windows.Forms.Label();
		this.labelCompanyName = new System.Windows.Forms.Label();
		this.button1 = new System.Windows.Forms.Button();
		this.logoPictureBox = new System.Windows.Forms.PictureBox();
		this.textBoxDescription = new System.Windows.Forms.TextBox();
		((System.ComponentModel.ISupportInitialize)this.logoPictureBox).BeginInit();
		base.SuspendLayout();
		this.labelProductName.AutoSize = true;
		this.labelProductName.Location = new System.Drawing.Point(216, 23);
		this.labelProductName.Name = "labelProductName";
		this.labelProductName.Size = new System.Drawing.Size(53, 12);
		this.labelProductName.TabIndex = 13;
		this.labelProductName.Text = "产品名称";
		this.labelVersion.AutoSize = true;
		this.labelVersion.Location = new System.Drawing.Point(216, 52);
		this.labelVersion.Name = "labelVersion";
		this.labelVersion.Size = new System.Drawing.Size(29, 12);
		this.labelVersion.TabIndex = 14;
		this.labelVersion.Text = "版本";
		this.labelCopyright.AutoSize = true;
		this.labelCopyright.Location = new System.Drawing.Point(216, 85);
		this.labelCopyright.Name = "labelCopyright";
		this.labelCopyright.Size = new System.Drawing.Size(29, 12);
		this.labelCopyright.TabIndex = 15;
		this.labelCopyright.Text = "版权";
		this.labelCompanyName.AutoSize = true;
		this.labelCompanyName.Location = new System.Drawing.Point(216, 121);
		this.labelCompanyName.Name = "labelCompanyName";
		this.labelCompanyName.Size = new System.Drawing.Size(53, 12);
		this.labelCompanyName.TabIndex = 16;
		this.labelCompanyName.Text = "公司信息";
		this.button1.Location = new System.Drawing.Point(339, 181);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(75, 23);
		this.button1.TabIndex = 18;
		this.button1.Text = "关闭";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click);
		this.logoPictureBox.Image = (System.Drawing.Image)resources.GetObject("logoPictureBox.Image");
		this.logoPictureBox.Location = new System.Drawing.Point(10, 59);
		this.logoPictureBox.Name = "logoPictureBox";
		this.logoPictureBox.Size = new System.Drawing.Size(191, 50);
		this.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
		this.logoPictureBox.TabIndex = 12;
		this.logoPictureBox.TabStop = false;
		this.textBoxDescription.Location = new System.Drawing.Point(134, 152);
		this.textBoxDescription.Name = "textBoxDescription";
		this.textBoxDescription.Size = new System.Drawing.Size(100, 21);
		this.textBoxDescription.TabIndex = 19;
		this.textBoxDescription.Visible = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(426, 216);
		base.Controls.Add(this.textBoxDescription);
		base.Controls.Add(this.button1);
		base.Controls.Add(this.labelCompanyName);
		base.Controls.Add(this.labelCopyright);
		base.Controls.Add(this.labelVersion);
		base.Controls.Add(this.labelProductName);
		base.Controls.Add(this.logoPictureBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "AboutBox1";
		base.Padding = new System.Windows.Forms.Padding(9, 8, 9, 8);
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "AboutBox1";
		((System.ComponentModel.ISupportInitialize)this.logoPictureBox).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
