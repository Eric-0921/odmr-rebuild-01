using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CControls;

[DefaultEvent("_TextChanged")]
public class CTextBox : UserControl
{
	private Color borderColor = Color.MediumSlateBlue;

	private Color borderFocusColor = Color.HotPink;

	private int borderSize = 2;

	private bool underlinedStyle;

	private bool isFocused;

	private int borderRadius;

	private Color placeholderColor = Color.DarkGray;

	private string placeholderText = "";

	private bool isPlaceholder;

	private bool isPasswordChar;

	private IContainer components;

	private TextBox textBox1;

	[Category("Custom")]
	public Color BorderColor
	{
		get
		{
			return borderColor;
		}
		set
		{
			borderColor = value;
			Invalidate();
		}
	}

	[Category("Custom")]
	public Color BorderFocusColor
	{
		get
		{
			return borderFocusColor;
		}
		set
		{
			borderFocusColor = value;
		}
	}

	[Category("Custom")]
	public int BorderSize
	{
		get
		{
			return borderSize;
		}
		set
		{
			if (value >= 1)
			{
				borderSize = value;
				Invalidate();
			}
		}
	}

	[Category("Custom")]
	public bool UnderlinedStyle
	{
		get
		{
			return underlinedStyle;
		}
		set
		{
			underlinedStyle = value;
			Invalidate();
		}
	}

	[Category("Custom")]
	public bool PasswordChar
	{
		get
		{
			return isPasswordChar;
		}
		set
		{
			isPasswordChar = value;
			if (!isPlaceholder)
			{
				textBox1.UseSystemPasswordChar = value;
			}
		}
	}

	[Category("RJ Code Advance")]
	public bool Multiline
	{
		get
		{
			return textBox1.Multiline;
		}
		set
		{
			textBox1.Multiline = value;
		}
	}

	[Category("Custom")]
	public override Color BackColor
	{
		get
		{
			return base.BackColor;
		}
		set
		{
			base.BackColor = value;
			textBox1.BackColor = value;
		}
	}

	[Category("Custom")]
	public override Color ForeColor
	{
		get
		{
			return base.ForeColor;
		}
		set
		{
			base.ForeColor = value;
			textBox1.ForeColor = value;
		}
	}

	[Category("Custom")]
	public override Font Font
	{
		get
		{
			return base.Font;
		}
		set
		{
			base.Font = value;
			textBox1.Font = value;
			if (base.DesignMode)
			{
				UpdateControlHeight();
			}
		}
	}

	[Category("Custom")]
	public string Texts
	{
		get
		{
			if (isPlaceholder)
			{
				return "";
			}
			return textBox1.Text;
		}
		set
		{
			textBox1.Text = value;
			SetPlaceholder();
		}
	}

	[Category("Custom")]
	public int BorderRadius
	{
		get
		{
			return borderRadius;
		}
		set
		{
			if (value >= 0)
			{
				borderRadius = value;
				Invalidate();
			}
		}
	}

	[Category("Custom")]
	public Color PlaceholderColor
	{
		get
		{
			return placeholderColor;
		}
		set
		{
			placeholderColor = value;
			if (isPlaceholder)
			{
				textBox1.ForeColor = value;
			}
		}
	}

	[Category("Custom")]
	public string PlaceholderText
	{
		get
		{
			return placeholderText;
		}
		set
		{
			placeholderText = value;
			textBox1.Text = "";
			SetPlaceholder();
		}
	}

	public event EventHandler _TextChanged;

	public CTextBox()
	{
		InitializeComponent();
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		if (base.DesignMode)
		{
			UpdateControlHeight();
		}
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		UpdateControlHeight();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Graphics graphics = e.Graphics;
		if (borderRadius > 1)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			Rectangle rect = Rectangle.Inflate(clientRectangle, -borderSize, -borderSize);
			int num = ((borderSize <= 0) ? 1 : borderSize);
			using GraphicsPath path = GetFigurePath(clientRectangle, borderRadius);
			using GraphicsPath path2 = GetFigurePath(rect, borderRadius - borderSize);
			using Pen pen = new Pen(base.Parent.BackColor, num);
			using Pen pen2 = new Pen(borderColor, borderSize);
			base.Region = new Region(path);
			if (borderRadius > 15)
			{
				SetTextBoxRoundedRegion();
			}
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			pen2.Alignment = PenAlignment.Center;
			if (isFocused)
			{
				pen2.Color = borderFocusColor;
			}
			if (underlinedStyle)
			{
				graphics.DrawPath(pen, path);
				graphics.SmoothingMode = SmoothingMode.None;
				graphics.DrawLine(pen2, 0, base.Height - 1, base.Width, base.Height - 1);
			}
			else
			{
				graphics.DrawPath(pen, path);
				graphics.DrawPath(pen2, path2);
			}
			return;
		}
		using Pen pen3 = new Pen(borderColor, borderSize);
		base.Region = new Region(base.ClientRectangle);
		pen3.Alignment = PenAlignment.Inset;
		if (isFocused)
		{
			pen3.Color = borderFocusColor;
		}
		if (underlinedStyle)
		{
			graphics.DrawLine(pen3, 0, base.Height - 1, base.Width, base.Height - 1);
		}
		else
		{
			graphics.DrawRectangle(pen3, 0f, 0f, (float)base.Width - 0.5f, (float)base.Height - 0.5f);
		}
	}

	private void SetPlaceholder()
	{
		if (string.IsNullOrWhiteSpace(textBox1.Text) && placeholderText != "")
		{
			isPlaceholder = true;
			textBox1.Text = placeholderText;
			textBox1.ForeColor = placeholderColor;
			if (isPasswordChar)
			{
				textBox1.UseSystemPasswordChar = false;
			}
		}
	}

	private void RemovePlaceholder()
	{
		if (isPlaceholder && placeholderText != "")
		{
			isPlaceholder = false;
			textBox1.Text = "";
			textBox1.ForeColor = ForeColor;
			if (isPasswordChar)
			{
				textBox1.UseSystemPasswordChar = true;
			}
		}
	}

	private GraphicsPath GetFigurePath(Rectangle rect, int radius)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		float num = (float)radius * 2f;
		graphicsPath.StartFigure();
		graphicsPath.AddArc(rect.X, rect.Y, num, num, 180f, 90f);
		graphicsPath.AddArc((float)rect.Right - num, rect.Y, num, num, 270f, 90f);
		graphicsPath.AddArc((float)rect.Right - num, (float)rect.Bottom - num, num, num, 0f, 90f);
		graphicsPath.AddArc(rect.X, (float)rect.Bottom - num, num, num, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	private void SetTextBoxRoundedRegion()
	{
		GraphicsPath figurePath;
		if (Multiline)
		{
			figurePath = GetFigurePath(textBox1.ClientRectangle, borderRadius - borderSize);
			textBox1.Region = new Region(figurePath);
		}
		else
		{
			figurePath = GetFigurePath(textBox1.ClientRectangle, borderSize * 2);
			textBox1.Region = new Region(figurePath);
		}
		figurePath.Dispose();
	}

	private void UpdateControlHeight()
	{
		if (!textBox1.Multiline)
		{
			int num = TextRenderer.MeasureText("Text", Font).Height + 1;
			textBox1.Multiline = true;
			textBox1.MinimumSize = new Size(0, num);
			textBox1.Multiline = false;
			base.Height = textBox1.Height + base.Padding.Top + base.Padding.Bottom;
		}
	}

	private void textBox1_TextChanged(object sender, EventArgs e)
	{
		if (this._TextChanged != null)
		{
			this._TextChanged(sender, e);
		}
	}

	private void textBox1_Click(object sender, EventArgs e)
	{
		OnClick(e);
	}

	private void textBox1_MouseEnter(object sender, EventArgs e)
	{
		OnMouseEnter(e);
	}

	private void textBox1_MouseLeave(object sender, EventArgs e)
	{
		OnMouseLeave(e);
	}

	private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
	{
		OnKeyPress(e);
	}

	private void textBox1_Enter(object sender, EventArgs e)
	{
		isFocused = true;
		Invalidate();
		RemovePlaceholder();
	}

	private void textBox1_Leave(object sender, EventArgs e)
	{
		isFocused = false;
		Invalidate();
		SetPlaceholder();
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
		this.textBox1 = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.textBox1.Location = new System.Drawing.Point(10, 7);
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(230, 15);
		this.textBox1.TabIndex = 0;
		this.textBox1.Click += new System.EventHandler(textBox1_Click);
		this.textBox1.TextChanged += new System.EventHandler(textBox1_TextChanged);
		this.textBox1.Enter += new System.EventHandler(textBox1_Enter);
		this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textBox1_KeyPress);
		this.textBox1.Leave += new System.EventHandler(textBox1_Leave);
		this.textBox1.MouseEnter += new System.EventHandler(textBox1_MouseEnter);
		this.textBox1.MouseLeave += new System.EventHandler(textBox1_MouseLeave);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.SystemColors.Window;
		base.Controls.Add(this.textBox1);
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		base.Margin = new System.Windows.Forms.Padding(4);
		base.Name = "CTextBox";
		base.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		base.Size = new System.Drawing.Size(250, 30);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
