using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CControls;

public class SevenSegmentArray : UserControl
{
	private SevenSegment[] segments;

	private int elementWidth = 10;

	private float italicFactor;

	private Color colorBackground = Color.DarkGray;

	private Color colorDark = Color.DimGray;

	private Color colorLight = Color.Red;

	private bool showDot = true;

	private Padding elementPadding;

	private string theValue;

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
			UpdateSegments();
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
			UpdateSegments();
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
			UpdateSegments();
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
			UpdateSegments();
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
			UpdateSegments();
		}
	}

	[Category("Custom")]
	public bool DecimalShow
	{
		get
		{
			return showDot;
		}
		set
		{
			showDot = value;
			UpdateSegments();
		}
	}

	[Category("Custom")]
	public int ArrayCount
	{
		get
		{
			return segments.Length;
		}
		set
		{
			if (value > 0 && value <= 100)
			{
				RecreateSegments(value);
			}
		}
	}

	[Category("Custom")]
	public Padding ElementPadding
	{
		get
		{
			return elementPadding;
		}
		set
		{
			elementPadding = value;
			UpdateSegments();
		}
	}

	[Category("Custom")]
	public string Value
	{
		get
		{
			return theValue;
		}
		set
		{
			theValue = value;
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i].CustomPattern = 0;
				segments[i].DecimalOn = false;
			}
			if (theValue == null)
			{
				return;
			}
			int num = 0;
			int num2 = theValue.Length - 1;
			while (num2 >= 0 && num < segments.Length)
			{
				if (theValue[num2] == '.')
				{
					segments[num].DecimalOn = true;
				}
				else
				{
					segments[num++].Value = theValue[num2].ToString();
				}
				num2--;
			}
		}
	}

	public SevenSegmentArray()
	{
		SuspendLayout();
		base.Name = "SevenSegmentArray";
		base.Size = new Size(100, 25);
		base.Resize += SevenSegmentArray_Resize;
		ResumeLayout(performLayout: false);
		base.TabStop = false;
		elementPadding = new Padding(4, 4, 4, 4);
		RecreateSegments(4);
	}

	private void RecreateSegments(int count)
	{
		if (segments != null)
		{
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i].Parent = null;
				segments[i].Dispose();
			}
		}
		if (count > 0)
		{
			segments = new SevenSegment[count];
			for (int j = 0; j < count; j++)
			{
				segments[j] = new SevenSegment();
				segments[j].Parent = this;
				segments[j].Top = 0;
				segments[j].Height = base.Height;
				segments[j].Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
				segments[j].Visible = true;
			}
			ResizeSegments();
			UpdateSegments();
			Value = theValue;
		}
	}

	private void ResizeSegments()
	{
		int num = base.Width / segments.Length;
		for (int i = 0; i < segments.Length; i++)
		{
			segments[i].Left = base.Width * (segments.Length - 1 - i) / segments.Length;
			segments[i].Width = num;
		}
	}

	private void UpdateSegments()
	{
		for (int i = 0; i < segments.Length; i++)
		{
			segments[i].ColorBackground = colorBackground;
			segments[i].ColorDark = colorDark;
			segments[i].ColorLight = colorLight;
			segments[i].ElementWidth = elementWidth;
			segments[i].ItalicFactor = italicFactor;
			segments[i].DecimalShow = showDot;
			segments[i].Padding = elementPadding;
		}
	}

	private void SevenSegmentArray_Resize(object sender, EventArgs e)
	{
		ResizeSegments();
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		e.Graphics.Clear(colorBackground);
	}
}
