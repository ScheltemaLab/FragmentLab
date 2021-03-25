using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;




namespace FragmentLab
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(true);
				AppDomain.CurrentDomain.UnhandledException += ProcessAppException;
				Application.ThreadException += ProcessThrException;
				Application.Run(new FragmentLab());
			}
			catch (Exception e)
			{
				DumpException(e);
			}
		}

		public enum ErrorReportLocation
		{
			APPLICATION_FOLDER,
			DESKTOP
		}

		public static void DumpException(Exception e, ErrorReportLocation location = ErrorReportLocation.DESKTOP)
		{
			string path = location == ErrorReportLocation.APPLICATION_FOLDER
				? AppDomain.CurrentDomain.BaseDirectory
				: Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			string dumpFile = Path.Combine(path, "crash_report_" + Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName) + "@" + DateTime.Now.ToString("yy-MM-dd_HH-mm-ss") + ".txt");

			System.Diagnostics.Debug.WriteLine(e.Message);
			System.Diagnostics.Debug.WriteLine(e.StackTrace);

			StreamWriter writer = new StreamWriter(dumpFile);
			writer.WriteLine("ERROR REPORT");
			writer.WriteLine("Fragment LAB version " + Application.ProductVersion);
			writer.WriteLine();

			writer.WriteLine(e.Message);
			writer.WriteLine("Thrown by: " + e.TargetSite);
			writer.WriteLine("Source obj/app: " + e.Source);
			writer.WriteLine();
			writer.WriteLine("---- INNER EXCEPTION ----");
			writer.WriteLine(e.InnerException);
			writer.WriteLine();
			writer.WriteLine("----   STACK TRACE   ----");
			writer.WriteLine(e.StackTrace);
			writer.Flush();
			writer.Close();

			MessageBox.Show(
					"An error occured and a report has been written to the desktop. Please send this to\n" +
					"  r.a.scheltema@uu.nl",
					"Error",
					MessageBoxButtons.OK, MessageBoxIcon.Error
				);
			Application.Exit();
		}

		public static void ProcessAppException(object sender, UnhandledExceptionEventArgs e)
		{
			DumpException((Exception)e.ExceptionObject);
		}

		public static void ProcessThrException(object sender, ThreadExceptionEventArgs e)
		{
			DumpException(e.Exception);
		}
	}
}
