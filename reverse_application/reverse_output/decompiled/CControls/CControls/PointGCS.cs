using System;

namespace CControls;

[Serializable]
public struct PointGCS
{
	private double lat;

	private double lon;

	public bool IsEmpty
	{
		get
		{
			if (lat == 0.0)
			{
				return lon == 0.0;
			}
			return false;
		}
	}

	public double Lat
	{
		get
		{
			return lat;
		}
		set
		{
			lat = value;
		}
	}

	public double Lon
	{
		get
		{
			return lon;
		}
		set
		{
			lon = value;
		}
	}

	public PointGCS(double lon, double lat)
	{
		this.lon = lon;
		this.lat = lat;
	}

	public void Clear()
	{
		lat = 0.0;
		lon = 0.0;
	}
}
