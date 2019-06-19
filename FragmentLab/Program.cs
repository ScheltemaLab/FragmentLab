using System;
using System.Collections.Generic;
using System.Linq;
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
				Application.SetCompatibleTextRenderingDefault(false);
				AppDomain.CurrentDomain.UnhandledException += HeckLib.utils.ProcessA.ProcessAppException;
				Application.ThreadException += HeckLib.utils.ProcessA.ProcessThrException;
				Application.Run(new FragmentLab());
			}
			catch (Exception e)
			{
				HeckLib.utils.ProcessA.DumpException(e);
			}
		}
	}
}
