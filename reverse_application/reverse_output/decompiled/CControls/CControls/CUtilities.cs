using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CControls;

public static class CUtilities
{
	public struct SYSTEMTIME
	{
		public short Year;

		public short Month;

		public short DayOfWeek;

		public short Day;

		public short Hour;

		public short Minute;

		public short Second;

		public short Miliseconds;
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
	public static extern bool SetSystemTime(ref SYSTEMTIME time);

	public static string BytesToHex(byte[] by, int startIndex, int len, bool addCRLF)
	{
		StringBuilder stringBuilder = new StringBuilder(512);
		for (int i = startIndex; i < startIndex + len; i++)
		{
			stringBuilder.AppendFormat("{0:X2} ", by[i]);
		}
		if (addCRLF)
		{
			stringBuilder.Append("\r\n");
		}
		return stringBuilder.ToString();
	}

	public static byte BcdEncode(byte by)
	{
		int num = by / 10;
		int num2 = by % 10;
		return (byte)(num * 16 + num2);
	}

	public static byte BcdDecode(byte bcd)
	{
		int num = bcd / 16;
		int num2 = bcd % 16;
		return (byte)(num * 10 + num2);
	}

	public static bool SetSystemClock(DateTime utcTime)
	{
		SYSTEMTIME time = new SYSTEMTIME
		{
			Year = (short)utcTime.Year,
			Month = (short)utcTime.Month,
			Day = (short)utcTime.Day,
			Hour = (short)utcTime.Hour,
			Minute = (short)utcTime.Minute,
			Second = (short)utcTime.Second,
			Miliseconds = (short)utcTime.Millisecond
		};
		return SetSystemTime(ref time);
	}

	public static string ExtractFilename(string filepath)
	{
		if (filepath.Trim().EndsWith("\\"))
		{
			return string.Empty;
		}
		int num = filepath.LastIndexOf('\\');
		if (num == -1)
		{
			if (File.Exists(Environment.CurrentDirectory + Path.DirectorySeparatorChar + filepath))
			{
				return filepath;
			}
			return string.Empty;
		}
		if (File.Exists(filepath))
		{
			return filepath.Substring(num + 1);
		}
		return string.Empty;
	}

	public static string ExtractDirname(string filepath, bool returnTwoLevelDir = false)
	{
		int num = filepath.LastIndexOf('\\');
		if (returnTwoLevelDir)
		{
			num = filepath.LastIndexOf('\\', num - 1);
		}
		return filepath.Substring(num + 1);
	}

	public static string ExtractVersion(byte low, byte high)
	{
		int num = low + (high << 8);
		int num2 = num / 10000;
		int num3 = num % 10000 / 100;
		int num4 = num % 100;
		return $"V{num2}.{num3}.{num4}";
	}

	public static void Sleep(int delayMs)
	{
		DateTime now = DateTime.Now;
		do
		{
			Thread.Sleep(5);
			Application.DoEvents();
		}
		while ((DateTime.Now - now).TotalMilliseconds < (double)delayMs);
	}
}
