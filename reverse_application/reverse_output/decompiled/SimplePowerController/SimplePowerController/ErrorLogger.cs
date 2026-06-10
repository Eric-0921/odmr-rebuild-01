using System;
using System.IO;

namespace SimplePowerController;

public class ErrorLogger
{
	private static readonly object thisLock = new object();

	private static string path = AppDomain.CurrentDomain.BaseDirectory + "Errors\\";

	public static void Write(string msg)
	{
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		lock (thisLock)
		{
			using StreamWriter streamWriter = File.AppendText($"{path}{DateTime.Now:yyyyMMdd}.Log");
			streamWriter.Write(DateTime.Now.ToString("HH:mm:ss.f  "));
			streamWriter.WriteLine(msg);
			streamWriter.Close();
		}
	}
}
