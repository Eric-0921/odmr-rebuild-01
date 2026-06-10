using System;
using System.Threading;
using System.Windows.Forms;

namespace SimplePowerController;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		Application.ThreadException += UI_ThreadException;
		Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		Application.Run(new FormMain());
	}

	private static void UI_ThreadException(object sender, ThreadExceptionEventArgs t)
	{
		try
		{
			ErrorLogger.Write(t.Exception.ToString());
		}
		catch
		{
			try
			{
				MessageBox.Show("不可恢复的Windows窗体异常，请截图保存错误信息并联系管理员！\r\n错误信息：\r\n" + t.Exception.ToString(), "错误");
			}
			finally
			{
				Application.Exit();
			}
		}
	}

	private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		try
		{
			ErrorLogger.Write(e.ExceptionObject.ToString());
		}
		catch
		{
			try
			{
				MessageBox.Show("不可恢复的非Windows窗体异常，请截图保存错误信息并联系管理员！\r\n错误信息：\r\n" + e.ExceptionObject.ToString(), "错误");
			}
			finally
			{
				Application.Exit();
			}
		}
	}
}
