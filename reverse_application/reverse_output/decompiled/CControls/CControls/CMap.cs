using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CControls;

public class CMap : UserControl
{
	public readonly double Earth_R = 6378137.0;

	private Bitmap imageMap;

	private List<List<PointGCS>> seriesCollection;

	private string[] seriesNames;

	private Color[] seriesColors;

	private List<Marker> markers;

	private List<PointGCS> routeVertices;

	private Dictionary<int, int> scaleMeters;

	private int scalePixels = 100;

	private int scaleLevel = 2;

	private PointGCS centerLonLat;

	private Point mouseDownLoc;

	private bool isMouseDown;

	private bool dataUpdate;

	private IContainer components;

	private PictureBox pictureBox1;

	private Button buttonZoomOut;

	private Button buttonZoomIn;

	private Button buttonLocation;

	private Timer timer1;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public List<Marker> Markers
	{
		get
		{
			return markers;
		}
		set
		{
			markers = value;
		}
	}

	public List<PointGCS> RouteVertices
	{
		get
		{
			return routeVertices;
		}
		set
		{
			routeVertices = value;
		}
	}

	public event EventHandler<PointGCS> MouseRightDown;

	public CMap()
	{
		scaleMeters = new Dictionary<int, int>(13)
		{
			{ 0, 5 },
			{ 1, 10 },
			{ 2, 20 },
			{ 3, 50 },
			{ 4, 100 },
			{ 5, 200 },
			{ 6, 500 },
			{ 7, 1000 },
			{ 8, 2000 },
			{ 9, 5000 },
			{ 10, 10000 },
			{ 11, 20000 },
			{ 12, 50000 }
		};
		InitializeComponent();
		pictureBox1.BackColor = SystemColors.GradientInactiveCaption;
		pictureBox1.MouseWheel += pictureBox1_MouseWheel;
		buttonLocation.Parent = pictureBox1;
		buttonZoomIn.Parent = pictureBox1;
		buttonZoomOut.Parent = pictureBox1;
	}

	public void Init(int count, string[] names, Color[] colors)
	{
		if (count <= 0)
		{
			throw new ArgumentException("序列数必须大于0！", "count");
		}
		if (names == null || names.Length != count)
		{
			throw new ArgumentException("序列名称参数错误！", "seriesName");
		}
		if (colors == null || colors.Length != count)
		{
			throw new ArgumentException("序列名称参数错误！", "colors");
		}
		seriesCollection = new List<List<PointGCS>>(count);
		while (count-- > 0)
		{
			seriesCollection.Add(new List<PointGCS>());
		}
		seriesNames = names;
		seriesColors = colors;
		markers = new List<Marker>();
		routeVertices = new List<PointGCS>();
		timer1.Start();
	}

	public void AddPoint(int index, PointGCS pt)
	{
		if (double.IsNaN(pt.Lon) || double.IsNaN(pt.Lat))
		{
			throw new ArgumentException("坐标不能为NaN！", "pt");
		}
		if (!pt.IsEmpty)
		{
			if (centerLonLat.IsEmpty)
			{
				centerLonLat = pt;
			}
			seriesCollection[index].Add(pt);
			dataUpdate = true;
		}
	}

	public void ClearPoints(int index)
	{
		seriesCollection[index].Clear();
		RefreshMap();
	}

	private void buttonLocation_Click(object sender, EventArgs e)
	{
		if (seriesCollection[0].Count > 0)
		{
			centerLonLat = seriesCollection[0].Last();
			RefreshMap();
		}
	}

	private void buttonZoomIn_Click(object sender, EventArgs e)
	{
		if (scaleLevel > 0)
		{
			scaleLevel--;
			RefreshMap();
		}
	}

	private void buttonZoomOut_Click(object sender, EventArgs e)
	{
		if (scaleLevel < 12)
		{
			scaleLevel++;
			RefreshMap();
		}
	}

	private void EasyMap_Resize(object sender, EventArgs e)
	{
		pictureBox1.Size = base.Size;
		if (pictureBox1.ClientSize.Width > 100 && pictureBox1.ClientSize.Height > 100)
		{
			if (imageMap != null)
			{
				imageMap.Dispose();
			}
			imageMap = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);
			RefreshMap();
		}
	}

	public void RefreshMap()
	{
		Graphics graphics = Graphics.FromImage(imageMap);
		Font font = new Font("Arial", 9f);
		SolidBrush solidBrush = new SolidBrush(Color.Black);
		Pen pen = new Pen(Color.Black, 1f);
		Pen pen2 = new Pen(Color.PowderBlue, 1f);
		try
		{
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.Clear(pictureBox1.BackColor);
			graphics.DrawImage(imageMap, 0, 0);
			int num = 100;
			int num2 = pictureBox1.Height / num;
			int num3 = pictureBox1.Width / num;
			for (int i = 0; i < num2; i++)
			{
				graphics.DrawLine(pen2, 1, num * (i + 1), pictureBox1.ClientSize.Width, num * (i + 1));
			}
			for (int j = 0; j < num3; j++)
			{
				graphics.DrawLine(pen2, num * (j + 1), 1, num * (j + 1), pictureBox1.ClientSize.Height);
			}
			string s = ((scaleMeters[scaleLevel] < 1000) ? $"{scaleMeters[scaleLevel]}m" : $"{scaleMeters[scaleLevel] / 1000}km");
			graphics.DrawString(s, font, solidBrush, new Point(38, 20));
			Point[] points = new Point[4]
			{
				new Point(1, 30),
				new Point(1, 35),
				new Point(100, 35),
				new Point(100, 30)
			};
			graphics.DrawLines(pen, points);
			Point centerPixel = new Point(pictureBox1.ClientSize.Width / 2, pictureBox1.ClientSize.Height / 2);
			double meterPerPixel = (double)scaleMeters[scaleLevel] / (double)scalePixels;
			if (seriesCollection != null)
			{
				for (int k = 0; k < seriesCollection.Count; k++)
				{
					Color color = seriesColors[k];
					using (new Pen(color))
					{
						foreach (PointGCS item in seriesCollection[k])
						{
							Point point = lonLatToPixel(centerLonLat, centerPixel, item, meterPerPixel);
							if (point.X > 0 && point.X < imageMap.Width && point.Y > 0 && point.Y < imageMap.Height)
							{
								imageMap.SetPixel(point.X, point.Y, color);
							}
						}
					}
					using SolidBrush brush = new SolidBrush(color);
					if (seriesCollection[k].Count > 0)
					{
						PointGCS lonLat = seriesCollection[k].Last();
						Point point2 = lonLatToPixel(centerLonLat, centerPixel, lonLat, meterPerPixel);
						if (point2.X > 0 && point2.X < imageMap.Width && point2.Y > 0 && point2.Y < imageMap.Height)
						{
							graphics.FillEllipse(brush, point2.X - 3, point2.Y - 3, 6, 6);
							graphics.DrawString(seriesNames[k], font, brush, new Point(point2.X + 10, point2.Y - 7));
						}
					}
				}
			}
			if (markers != null && markers.Count > 0)
			{
				if (centerLonLat.IsEmpty)
				{
					centerLonLat = markers[0].Pt;
				}
				for (int l = 0; l < markers.Count; l++)
				{
					using SolidBrush brush2 = new SolidBrush(Markers[l].Color);
					Point point3 = lonLatToPixel(centerLonLat, centerPixel, markers[l].Pt, meterPerPixel);
					new Point(point3.X - 7, point3.Y - 18);
					new Point(point3.X + 7, point3.Y - 18);
					Rectangle rect = new Rectangle(point3.X - 10, point3.Y - 35, 20, 20);
					float num4 = 130f;
					float sweepAngle = 540f - 2f * num4;
					GraphicsPath graphicsPath = new GraphicsPath();
					graphicsPath.StartFigure();
					graphicsPath.AddBezier(point3.X, point3.Y, point3.X - 2, point3.Y - 8, point3.X - 5, point3.Y - 16, point3.X - 7, point3.Y - 18);
					graphicsPath.AddArc(rect, num4, sweepAngle);
					graphicsPath.AddBezier(point3.X + 7, point3.Y - 18, point3.X + 5, point3.Y - 16, point3.X + 2, point3.Y - 8, point3.X, point3.Y);
					graphicsPath.CloseFigure();
					graphics.DrawPath(pen, graphicsPath);
					graphics.FillPath(brush2, graphicsPath);
				}
			}
			if (routeVertices != null && routeVertices.Count > 1)
			{
				if (centerLonLat.IsEmpty)
				{
					centerLonLat = routeVertices[0];
				}
				Point[] array = new Point[routeVertices.Count];
				for (int m = 0; m < routeVertices.Count; m++)
				{
					array[m] = lonLatToPixel(centerLonLat, centerPixel, routeVertices[m], meterPerPixel);
				}
				graphics.DrawLines(pen, array);
			}
			pictureBox1.Image = imageMap;
		}
		finally
		{
			graphics.Dispose();
			font.Dispose();
			solidBrush.Dispose();
			pen.Dispose();
			pen2.Dispose();
		}
	}

	private Point lonLatToPixel(PointGCS centerLonLat, Point centerPixel, PointGCS lonLat, double meterPerPixel)
	{
		double num = centerLonLat.Lat / 180.0 * Math.PI;
		double num2 = centerLonLat.Lon / 180.0 * Math.PI;
		double num3 = lonLat.Lat / 180.0 * Math.PI;
		double num4 = lonLat.Lon / 180.0 * Math.PI;
		double num5 = Math.Acos(Math.Cos(num) * Math.Cos(num3) * Math.Cos(num2 - num4) + Math.Sin(num) * Math.Sin(num3));
		if (double.IsNaN(num5))
		{
			return centerPixel;
		}
		double num6 = num5 * Earth_R;
		double num7 = (centerLonLat.Lat - lonLat.Lat) * Math.PI / 180.0 * Earth_R;
		double num8 = ((Math.Pow(num6, 2.0) <= Math.Pow(num7, 2.0)) ? 0.0 : Math.Sqrt(Math.Pow(num6, 2.0) - Math.Pow(num7, 2.0)));
		Point result = new Point
		{
			Y = (int)((double)centerPixel.Y + num7 / meterPerPixel)
		};
		if (lonLat.Lon > centerLonLat.Lon)
		{
			result.X = (int)((double)centerPixel.X + num8 / meterPerPixel);
		}
		else
		{
			result.X = (int)((double)centerPixel.X - num8 / meterPerPixel);
		}
		return result;
	}

	public double CalculateDistance(PointGCS pt1, PointGCS pt2)
	{
		double earth_R = Earth_R;
		double num = pt1.Lat / 180.0 * Math.PI;
		double num2 = pt1.Lon / 180.0 * Math.PI;
		double num3 = pt2.Lat / 180.0 * Math.PI;
		double num4 = pt2.Lon / 180.0 * Math.PI;
		double num5 = (num3 - num) * earth_R;
		double num6 = (num4 - num2) * earth_R * Math.Cos(num);
		return Math.Sqrt(num6 * num6 + num5 * num5);
	}

	private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
	{
		if (e.Delta > 0)
		{
			if (scaleLevel > 0)
			{
				scaleLevel--;
				RefreshMap();
			}
		}
		else if (scaleLevel < 12)
		{
			scaleLevel++;
			RefreshMap();
		}
	}

	private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			if (!centerLonLat.IsEmpty)
			{
				isMouseDown = true;
				mouseDownLoc = e.Location;
				pictureBox1.Cursor = Cursors.Hand;
			}
		}
		else if (e.Button == MouseButtons.Right && this.MouseRightDown != null)
		{
			if (centerLonLat.IsEmpty)
			{
				MessageBox.Show("地图上没有有效的坐标点，无法定位！");
				return;
			}
			Point point = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
			double num = (double)scaleMeters[scaleLevel] / (double)scalePixels;
			int num2 = e.X - point.X;
			int num3 = e.Y - point.Y;
			double num4 = (double)num2 * num;
			double num5 = (double)num3 * num;
			PointGCS e2 = new PointGCS
			{
				Lat = Math.Round(centerLonLat.Lat - num5 / (Math.PI * Earth_R) * 180.0, 6),
				Lon = Math.Round(centerLonLat.Lon + num4 / (Math.PI * Earth_R * Math.Cos(Math.PI * centerLonLat.Lat / 180.0)) * 180.0, 6)
			};
			this.MouseRightDown(this, e2);
		}
	}

	private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left && isMouseDown)
		{
			pictureBox1.Cursor = base.ParentForm.Cursor;
			isMouseDown = false;
			if (e.X >= 0 && e.X <= pictureBox1.Width && e.Y >= 0 && e.Y <= pictureBox1.Height)
			{
				int num = e.X - mouseDownLoc.X;
				int num2 = e.Y - mouseDownLoc.Y;
				double num3 = (double)scaleMeters[scaleLevel] / (double)scalePixels;
				double num4 = (double)num * num3;
				double num5 = (double)num2 * num3;
				centerLonLat = new PointGCS
				{
					Lat = centerLonLat.Lat + num5 / (Math.PI * Earth_R) * 180.0,
					Lon = centerLonLat.Lon - num4 / (Math.PI * Earth_R * Math.Cos(Math.PI * centerLonLat.Lat / 180.0)) * 180.0
				};
				RefreshMap();
			}
		}
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		if (dataUpdate)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			RefreshMap();
			stopwatch.Stop();
			dataUpdate = false;
		}
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
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CControls.CMap));
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.buttonZoomOut = new System.Windows.Forms.Button();
		this.buttonZoomIn = new System.Windows.Forms.Button();
		this.buttonLocation = new System.Windows.Forms.Button();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		base.SuspendLayout();
		this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pictureBox1.Location = new System.Drawing.Point(0, 0);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(285, 216);
		this.pictureBox1.TabIndex = 1;
		this.pictureBox1.TabStop = false;
		this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(pictureBox1_MouseDown);
		this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(pictureBox1_MouseUp);
		this.buttonZoomOut.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.buttonZoomOut.BackColor = System.Drawing.Color.Transparent;
		this.buttonZoomOut.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonZoomOut.BackgroundImage");
		this.buttonZoomOut.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.buttonZoomOut.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
		this.buttonZoomOut.Location = new System.Drawing.Point(241, 150);
		this.buttonZoomOut.Name = "buttonZoomOut";
		this.buttonZoomOut.Size = new System.Drawing.Size(32, 32);
		this.buttonZoomOut.TabIndex = 6;
		this.buttonZoomOut.UseVisualStyleBackColor = false;
		this.buttonZoomOut.Click += new System.EventHandler(buttonZoomOut_Click);
		this.buttonZoomIn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.buttonZoomIn.BackColor = System.Drawing.Color.Transparent;
		this.buttonZoomIn.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonZoomIn.BackgroundImage");
		this.buttonZoomIn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.buttonZoomIn.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
		this.buttonZoomIn.Location = new System.Drawing.Point(241, 112);
		this.buttonZoomIn.Name = "buttonZoomIn";
		this.buttonZoomIn.Size = new System.Drawing.Size(32, 32);
		this.buttonZoomIn.TabIndex = 5;
		this.buttonZoomIn.UseVisualStyleBackColor = false;
		this.buttonZoomIn.Click += new System.EventHandler(buttonZoomIn_Click);
		this.buttonLocation.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.buttonLocation.BackColor = System.Drawing.Color.Transparent;
		this.buttonLocation.BackgroundImage = (System.Drawing.Image)resources.GetObject("buttonLocation.BackgroundImage");
		this.buttonLocation.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.buttonLocation.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
		this.buttonLocation.Location = new System.Drawing.Point(241, 74);
		this.buttonLocation.Name = "buttonLocation";
		this.buttonLocation.Size = new System.Drawing.Size(32, 32);
		this.buttonLocation.TabIndex = 4;
		this.buttonLocation.UseVisualStyleBackColor = false;
		this.buttonLocation.Click += new System.EventHandler(buttonLocation_Click);
		this.timer1.Interval = 1000;
		this.timer1.Tick += new System.EventHandler(timer1_Tick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.buttonZoomOut);
		base.Controls.Add(this.buttonZoomIn);
		base.Controls.Add(this.buttonLocation);
		base.Controls.Add(this.pictureBox1);
		base.Name = "EasyMap";
		base.Size = new System.Drawing.Size(285, 216);
		base.Resize += new System.EventHandler(EasyMap_Resize);
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		base.ResumeLayout(false);
	}
}
