using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CControls;

internal class CRadioButton : RadioButton
{
	private Color checkedColor = Color.MediumSlateBlue;

	private Color unCheckedColor = Color.Gray;

	[Category("Custom")]
	public Color CheckedColor
	{
		get
		{
			return checkedColor;
		}
		set
		{
			checkedColor = value;
			Invalidate();
		}
	}

	[Category("Custom")]
	public Color UnCheckedColor
	{
		get
		{
			return unCheckedColor;
		}
		set
		{
			unCheckedColor = value;
			Invalidate();
		}
	}

	public CRadioButton()
	{
		MinimumSize = new Size(0, 21);
		base.Padding = new Padding(10, 0, 0, 0);
	}

	protected override void OnPaint(PaintEventArgs pevent)
	{
		Graphics graphics = pevent.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		float num = 18f;
		float num2 = 12f;
		RectangleF rect = new RectangleF
		{
			X = 0.5f,
			Y = ((float)base.Height - num) / 2f,
			Width = num,
			Height = num
		};
		RectangleF rect2 = new RectangleF
		{
			X = rect.X + (rect.Width - num2) / 2f,
			Y = ((float)base.Height - num2) / 2f,
			Width = num2,
			Height = num2
		};
		using Pen pen = new Pen(checkedColor, 1.6f);
		using SolidBrush brush = new SolidBrush(checkedColor);
		using SolidBrush brush2 = new SolidBrush(ForeColor);
		graphics.Clear(BackColor);
		if (base.Checked)
		{
			graphics.DrawEllipse(pen, rect);
			graphics.FillEllipse(brush, rect2);
		}
		else
		{
			pen.Color = unCheckedColor;
			graphics.DrawEllipse(pen, rect);
		}
		graphics.DrawString(Text, Font, brush2, num + 8f, (base.Height - TextRenderer.MeasureText(Text, Font).Height) / 2);
	}
}
