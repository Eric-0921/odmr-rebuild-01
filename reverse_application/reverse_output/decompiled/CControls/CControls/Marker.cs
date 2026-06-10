using System;
using System.Drawing;

namespace CControls;

public struct Marker : IEquatable<Marker>
{
	public PointGCS Pt;

	public Color Color;

	public override bool Equals(object obj)
	{
		if (obj is Marker other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(Marker other)
	{
		if (Pt.Lat == other.Pt.Lat && Pt.Lon == other.Pt.Lon)
		{
			return Color == other.Color;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Pt.Lat, Pt.Lon, Color.GetHashCode()).GetHashCode();
	}
}
