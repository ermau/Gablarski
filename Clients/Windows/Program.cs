using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Kennedy.ManagedHooks;

namespace Gablarski.Clients.Windows
{
	static class Program
	{
		public static readonly KeyboardHook KHook = new KeyboardHook();
		public static readonly MouseHook MHook = new MouseHook();

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main ()
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				KHook.UninstallHook();
				//MHook.UninstallHook();
				//TaskDialog.Show ((e.ExceptionObject as Exception).ToDisplayString(), "Unexpected Error", "Unexpected Error",
				//                 TaskDialogStandardIcon.Error);

				MessageBox.Show ("Unexpected error" + Environment.NewLine + (e.ExceptionObject as Exception).ToDisplayString(),
				                 "Unexpected error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			};

			string glogs = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Desktop), "glogs");
			Directory.CreateDirectory (glogs);
			var logger = new TextWriterTraceListener (File.Open (Path.Combine(glogs, DateTime.Now.Ticks + ".txt"), FileMode.Append));
			logger.TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime | TraceOptions.Timestamp;
			Trace.Listeners.Add (logger);

			KHook.InstallHook();
			//MHook.InstallHook();

			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);

			try
			{
				var m = new MainForm();
				m.Show();
				if (m.ShowConnect (true))
					Application.Run (m);
			}
			finally
			{
				Settings.SaveSettings();
				Persistance.CurrentSession.Flush();
				KHook.UninstallHook();
				//MHook.UninstallHook();
			}
		}
	}
}