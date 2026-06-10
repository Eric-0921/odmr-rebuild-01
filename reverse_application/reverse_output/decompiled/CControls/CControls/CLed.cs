using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CControls;

public class CLed : Control
{
	public CLed()
	{
		MinimumSize = new Size(5, 5);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		RectangleF rect = new RectangleF
		{
			X = 0.5f,
			Y = 0.5f,
			Width = (float)base.Width - 1f,
			Height = (float)base.Height - 1f
		};
		using SolidBrush brush = new SolidBrush(ForeColor);
		graphics.Clear(BackColor);
		graphics.FillEllipse(brush, rect);
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		base.Height = base.Width;
	}
}
