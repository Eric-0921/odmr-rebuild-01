using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CControls;

public class CButton : Button
{
	private int borderSize;

	private int borderRadius;

	private Color borderColor = Color.PaleVioletRed;

	[Category("Custom")]
	public int BorderSize
	{
		get
		{
			return borderSize;
		}
		set
		{
			borderSize = value;
			Invalidate();
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
			borderRadius = value;
			Invalidate();
		}
	}

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
	public Color BackgroundColor
	{
		get
		{
			return BackColor;
		}
		set
		{
			BackColor = value;
		}
	}

	[Category("Custom")]
	public Color TextColor
	{
		get
		{
			return ForeColor;
		}
		set
		{
			ForeColor = value;
		}
	}

	public CButton()
	{
		base.FlatStyle = FlatStyle.Flat;
		base.FlatAppearance.BorderSize = 0;
		base.Size = new Size(150, 40);
		BackColor = Color.MediumSlateBlue;
		ForeColor = Color.White;
		base.Resize += Button_Resize;
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

	protected override void OnPaint(PaintEventArgs pevent)
	{
		base.OnPaint(pevent);
		Rectangle clientRectangle = base.ClientRectangle;
		Rectangle rect = Rectangle.Inflate(clientRectangle, -borderSize, -borderSize);
		int num = 2;
		if (borderSize > 0)
		{
			num = borderSize;
		}
		if (borderRadius > 2)
		{
			using (GraphicsPath path = GetFigurePath(clientRectangle, borderRadius))
			{
				using GraphicsPath path2 = GetFigurePath(rect, borderRadius - borderSize);
				using Pen pen = new Pen(base.Parent.BackColor, num);
				using Pen pen2 = new Pen(borderColor, borderSize);
				pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				base.Region = new Region(path);
				pevent.Graphics.DrawPath(pen, path);
				if (borderSize >= 1)
				{
					pevent.Graphics.DrawPath(pen2, path2);
				}
				return;
			}
		}
		pevent.Graphics.SmoothingMode = SmoothingMode.None;
		base.Region = new Region(clientRectangle);
		if (borderSize >= 1)
		{
			using (Pen pen3 = new Pen(borderColor, borderSize))
			{
				pen3.Alignment = PenAlignment.Inset;
				pevent.Graphics.DrawRectangle(pen3, 0, 0, base.Width - 1, base.Height - 1);
			}
		}
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		base.Parent.BackColorChanged += Container_BackColorChanged;
	}

	private void Container_BackColorChanged(object sender, EventArgs e)
	{
		Invalidate();
	}

	private void Button_Resize(object sender, EventArgs e)
	{
		if (borderRadius > base.Height)
		{
			borderRadius = base.Height;
		}
	}
}
