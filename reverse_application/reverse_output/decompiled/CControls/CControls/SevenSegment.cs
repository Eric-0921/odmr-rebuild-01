using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CControls;

public class SevenSegment : UserControl
{
	public enum ValuePattern
	{
		None = 0,
		Zero = 119,
		One = 36,
		Two = 93,
		Three = 109,
		Four = 46,
		Five = 107,
		Six = 123,
		Seven = 37,
		Eight = 127,
		Nine = 111,
		A = 63,
		B = 122,
		C = 83,
		c = 88,
		D = 124,
		E = 91,
		F = 27,
		G = 115,
		H = 62,
		h = 58,
		i = 32,
		J = 116,
		L = 82,
		N = 56,
		o = 120,
		P = 31,
		Q = 47,
		R = 24,
		T = 90,
		U = 118,
		u = 112,
		Y = 110,
		Dash = 8,
		Equals = 72,
		Degrees = 15,
		Apostrophe = 2,
		Quote = 6,
		RBracket = 101,
		Underscore = 64,
		Identical = 73,
		Not = 40
	}

	private Point[][] segPoints;

	private int gridHeight = 80;

	private int gridWidth = 48;

	private int elementWidth = 10;

	private float italicFactor;

	private Color colorBackground = Color.DarkGray;

	private Color colorDark = Color.DimGray;

	private Color colorLight = Color.Red;

	private string theValue;

	private bool showDot = true;

	private bool dotOn;

	private bool showColon;

	private bool colonOn;

	private int customPattern;

	[Category("Custom")]
	public Color ColorBackground
	{
		get
		{
			return colorBackground;
		}
		set
		{
			colorBackground = value;
			Invalidate();
		}
	}

	[Category("Custom")]
	public Color ColorDark
	{
		get
		{
			return colorDark;
		}
		set
		{
			colorDark = value;
			Invalidate();
		}
	}

	[Category("Custom")]
	public Color ColorLight
	{
		get
		{
			return colorLight;
		}
		set
		{
			colorLight = value;
			Invalidate();
		}
	}

	[Category("Custom")]
	public int ElementWidth
	{
		get
		{
			return elementWidth;
		}
		set
		{
			elementWidth = value;
			RecalculatePoints();
			Invalidate();
		}
	}

	[Category("Custom")]
	public float ItalicFactor
	{
		get
		{
			return italicFactor;
		}
		set
		{
			italicFactor = value;
			Invalidate();
		}
	}

	public string Value
	{
		get
		{
			return theValue;
		}
		set
		{
			customPattern = 0;
			theValue = value;
			Invalidate();
			if (value == null || value.Length == 0)
			{
				return;
			}
			if (int.TryParse(value, out var result))
			{
				if (result > 9)
				{
					result = 9;
				}
				if (result < 0)
				{
					result = 0;
				}
				switch (result)
				{
				case 0:
					customPattern = 119;
					break;
				case 1:
					customPattern = 36;
					break;
				case 2:
					customPattern = 93;
					break;
				case 3:
					customPattern = 109;
					break;
				case 4:
					customPattern = 46;
					break;
				case 5:
					customPattern = 107;
					break;
				case 6:
					customPattern = 123;
					break;
				case 7:
					customPattern = 37;
					break;
				case 8:
					customPattern = 127;
					break;
				case 9:
					customPattern = 111;
					break;
				}
				return;
			}
			switch (value[0])
			{
			case 'A':
			case 'a':
				customPattern = 63;
				break;
			case 'B':
			case 'b':
				customPattern = 122;
				break;
			case 'C':
				customPattern = 83;
				break;
			case 'c':
				customPattern = 88;
				break;
			case 'D':
			case 'd':
				customPattern = 124;
				break;
			case 'E':
			case 'e':
				customPattern = 91;
				break;
			case 'F':
			case 'f':
				customPattern = 27;
				break;
			case 'G':
			case 'g':
				customPattern = 115;
				break;
			case 'H':
				customPattern = 62;
				break;
			case 'h':
				customPattern = 58;
				break;
			case 'I':
				customPattern = 36;
				break;
			case 'i':
				customPattern = 32;
				break;
			case 'J':
			case 'j':
				customPattern = 116;
				break;
			case 'L':
			case 'l':
				customPattern = 82;
				break;
			case 'N':
			case 'n':
				customPattern = 56;
				break;
			case 'O':
				customPattern = 119;
				break;
			case 'o':
				customPattern = 120;
				break;
			case 'P':
			case 'p':
				customPattern = 31;
				break;
			case 'Q':
			case 'q':
				customPattern = 47;
				break;
			case 'R':
			case 'r':
				customPattern = 24;
				break;
			case 'S':
			case 's':
				customPattern = 107;
				break;
			case 'T':
			case 't':
				customPattern = 90;
				break;
			case 'U':
				customPattern = 118;
				break;
			case 'u':
			case 'µ':
			case 'μ':
				customPattern = 112;
				break;
			case 'Y':
			case 'y':
				customPattern = 110;
				break;
			case '-':
				customPattern = 8;
				break;
			case '=':
				customPattern = 72;
				break;
			case '°':
				customPattern = 15;
				break;
			case '\'':
				customPattern = 2;
				break;
			case '"':
				customPattern = 6;
				break;
			case '[':
			case '{':
				customPattern = 83;
				break;
			case ']':
			case '}':
				customPattern = 101;
				break;
			case '_':
				customPattern = 64;
				break;
			case '≡':
				customPattern = 73;
				break;
			case '¬':
				customPattern = 40;
				break;
			}
		}
	}

	public int CustomPattern
	{
		get
		{
			return customPattern;
		}
		set
		{
			customPattern = value;
			Invalidate();
		}
	}

	public bool DecimalShow
	{
		get
		{
			return showDot;
		}
		set
		{
			showDot = value;
			Invalidate();
		}
	}

	public bool DecimalOn
	{
		get
		{
			return dotOn;
		}
		set
		{
			dotOn = value;
			Invalidate();
		}
	}

	public bool ColonShow
	{
		get
		{
			return showColon;
		}
		set
		{
			showColon = value;
			Invalidate();
		}
	}

	public bool ColonOn
	{
		get
		{
			return colonOn;
		}
		set
		{
			colonOn = value;
			Invalidate();
		}
	}

	public SevenSegment()
	{
		SuspendLayout();
		base.Name = "SevenSegment";
		base.Size = new Size(32, 64);
		base.Paint += SevenSegment_Paint;
		base.Resize += SevenSegment_Resize;
		ResumeLayout(performLayout: false);
		base.TabStop = false;
		base.Padding = new Padding(4, 4, 4, 4);
		DoubleBuffered = true;
		segPoints = new Point[7][];
		for (int i = 0; i < 7; i++)
		{
			segPoints[i] = new Point[6];
		}
		RecalculatePoints();
	}

	private void RecalculatePoints()
	{
		int num = gridHeight / 2;
		int num2 = elementWidth / 2;
		int num3 = 0;
		segPoints[num3][0].X = elementWidth + 1;
		segPoints[num3][0].Y = 0;
		segPoints[num3][1].X = gridWidth - elementWidth - 1;
		segPoints[num3][1].Y = 0;
		segPoints[num3][2].X = gridWidth - num2 - 1;
		segPoints[num3][2].Y = num2;
		segPoints[num3][3].X = gridWidth - elementWidth - 1;
		segPoints[num3][3].Y = elementWidth;
		segPoints[num3][4].X = elementWidth + 1;
		segPoints[num3][4].Y = elementWidth;
		segPoints[num3][5].X = num2 + 1;
		segPoints[num3][5].Y = num2;
		num3++;
		segPoints[num3][0].X = 0;
		segPoints[num3][0].Y = elementWidth + 1;
		segPoints[num3][1].X = num2;
		segPoints[num3][1].Y = num2 + 1;
		segPoints[num3][2].X = elementWidth;
		segPoints[num3][2].Y = elementWidth + 1;
		segPoints[num3][3].X = elementWidth;
		segPoints[num3][3].Y = num - num2 - 1;
		segPoints[num3][4].X = 4;
		segPoints[num3][4].Y = num - 1;
		segPoints[num3][5].X = 0;
		segPoints[num3][5].Y = num - 1;
		num3++;
		segPoints[num3][0].X = gridWidth - elementWidth;
		segPoints[num3][0].Y = elementWidth + 1;
		segPoints[num3][1].X = gridWidth - num2;
		segPoints[num3][1].Y = num2 + 1;
		segPoints[num3][2].X = gridWidth;
		segPoints[num3][2].Y = elementWidth + 1;
		segPoints[num3][3].X = gridWidth;
		segPoints[num3][3].Y = num - 1;
		segPoints[num3][4].X = gridWidth - 4;
		segPoints[num3][4].Y = num - 1;
		segPoints[num3][5].X = gridWidth - elementWidth;
		segPoints[num3][5].Y = num - num2 - 1;
		num3++;
		segPoints[num3][0].X = elementWidth + 1;
		segPoints[num3][0].Y = num - num2;
		segPoints[num3][1].X = gridWidth - elementWidth - 1;
		segPoints[num3][1].Y = num - num2;
		segPoints[num3][2].X = gridWidth - 5;
		segPoints[num3][2].Y = num;
		segPoints[num3][3].X = gridWidth - elementWidth - 1;
		segPoints[num3][3].Y = num + num2;
		segPoints[num3][4].X = elementWidth + 1;
		segPoints[num3][4].Y = num + num2;
		segPoints[num3][5].X = 5;
		segPoints[num3][5].Y = num;
		num3++;
		segPoints[num3][0].X = 0;
		segPoints[num3][0].Y = num + 1;
		segPoints[num3][1].X = 4;
		segPoints[num3][1].Y = num + 1;
		segPoints[num3][2].X = elementWidth;
		segPoints[num3][2].Y = num + num2 + 1;
		segPoints[num3][3].X = elementWidth;
		segPoints[num3][3].Y = gridHeight - elementWidth - 1;
		segPoints[num3][4].X = num2;
		segPoints[num3][4].Y = gridHeight - num2 - 1;
		segPoints[num3][5].X = 0;
		segPoints[num3][5].Y = gridHeight - elementWidth - 1;
		num3++;
		segPoints[num3][0].X = gridWidth - elementWidth;
		segPoints[num3][0].Y = num + num2 + 1;
		segPoints[num3][1].X = gridWidth - 4;
		segPoints[num3][1].Y = num + 1;
		segPoints[num3][2].X = gridWidth;
		segPoints[num3][2].Y = num + 1;
		segPoints[num3][3].X = gridWidth;
		segPoints[num3][3].Y = gridHeight - elementWidth - 1;
		segPoints[num3][4].X = gridWidth - num2;
		segPoints[num3][4].Y = gridHeight - num2 - 1;
		segPoints[num3][5].X = gridWidth - elementWidth;
		segPoints[num3][5].Y = gridHeight - elementWidth - 1;
		num3++;
		segPoints[num3][0].X = elementWidth + 1;
		segPoints[num3][0].Y = gridHeight - elementWidth;
		segPoints[num3][1].X = gridWidth - elementWidth - 1;
		segPoints[num3][1].Y = gridHeight - elementWidth;
		segPoints[num3][2].X = gridWidth - num2 - 1;
		segPoints[num3][2].Y = gridHeight - num2;
		segPoints[num3][3].X = gridWidth - elementWidth - 1;
		segPoints[num3][3].Y = gridHeight;
		segPoints[num3][4].X = elementWidth + 1;
		segPoints[num3][4].Y = gridHeight;
		segPoints[num3][5].X = num2 + 1;
		segPoints[num3][5].Y = gridHeight - num2;
	}

	private void SevenSegment_Resize(object sender, EventArgs e)
	{
		Invalidate();
	}

	protected override void OnPaddingChanged(EventArgs e)
	{
		base.OnPaddingChanged(e);
		Invalidate();
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		e.Graphics.Clear(colorBackground);
	}

	private void SevenSegment_Paint(object sender, PaintEventArgs e)
	{
		int num = customPattern;
		Brush brush = new SolidBrush(colorLight);
		Brush brush2 = new SolidBrush(colorDark);
		int num2 = gridWidth / 4;
		RectangleF srcrect = ((!showColon) ? new RectangleF(0f, 0f, gridWidth, gridHeight) : new RectangleF(0f, 0f, gridWidth + num2, gridHeight));
		RectangleF dstrect = new RectangleF(base.Padding.Left, base.Padding.Top, base.Width - base.Padding.Left - base.Padding.Right, base.Height - base.Padding.Top - base.Padding.Bottom);
		GraphicsContainer container = e.Graphics.BeginContainer(dstrect, srcrect, GraphicsUnit.Pixel);
		Matrix matrix = new Matrix();
		matrix.Shear(italicFactor, 0f);
		e.Graphics.Transform = matrix;
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;
		e.Graphics.FillPolygon(((num & 1) == 1) ? brush : brush2, segPoints[0]);
		e.Graphics.FillPolygon(((num & 2) == 2) ? brush : brush2, segPoints[1]);
		e.Graphics.FillPolygon(((num & 4) == 4) ? brush : brush2, segPoints[2]);
		e.Graphics.FillPolygon(((num & 8) == 8) ? brush : brush2, segPoints[3]);
		e.Graphics.FillPolygon(((num & 0x10) == 16) ? brush : brush2, segPoints[4]);
		e.Graphics.FillPolygon(((num & 0x20) == 32) ? brush : brush2, segPoints[5]);
		e.Graphics.FillPolygon(((num & 0x40) == 64) ? brush : brush2, segPoints[6]);
		if (showDot)
		{
			e.Graphics.FillEllipse(dotOn ? brush : brush2, gridWidth - 1, gridHeight - elementWidth + 1, elementWidth, elementWidth);
		}
		if (showColon)
		{
			e.Graphics.FillEllipse(colonOn ? brush : brush2, gridWidth + num2 - 4, gridHeight / 4 - elementWidth + 8, elementWidth, elementWidth);
			e.Graphics.FillEllipse(colonOn ? brush : brush2, gridWidth + num2 - 4, gridHeight * 3 / 4 - elementWidth + 4, elementWidth, elementWidth);
		}
		e.Graphics.EndContainer(container);
	}
}
