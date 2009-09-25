using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Gablarski.Clients.Windows
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main ()
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				MessageBox.Show ("Unexpected error" + Environment.NewLine + (e.ExceptionObject as Exception).ToDisplayString(),
				                 "Unexpected error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			};

			//string glogs = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Desktop), "glogs");
			//Directory.CreateDirectory (glogs);
			//var logger = new TextWriterTraceListener (File.Open (Path.Combine(glogs, DateTime.Now.Ticks + ".txt"), FileMode.Append));
			//logger.TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime | TraceOptions.Timestamp;
			//Trace.Listeners.Add (logger);

			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);

			var m = new MainForm();
			m.Show();
			if (m.ShowConnect (true))
				Application.Run (m);

			Settings.SaveSettings();
		}
	}
}