using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

namespace Gablarski.Clients.Windows
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main (string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				MessageBox.Show ("Unexpected error" + Environment.NewLine + (e.ExceptionObject as Exception).ToDisplayString(),
				                 "Unexpected error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			};

			if (Settings.FirstRun)
			{
				try
				{
					if (Settings.EnableGablarskiURLs)
					{
						try
						{
							Process p = new Process
							{
								StartInfo = new ProcessStartInfo ("cmd.exe", "/C ftype gablarski=\"" + Path.Combine (Environment.CurrentDirectory, Process.GetCurrentProcess ().ProcessName) + ".exe\" \"%1\"")
								{
									Verb = "runas",
								}
							};
							p.Start ();
							p.WaitForExit ();
						}
						catch (Win32Exception)
						{
						}
					}

					Settings.FirstRun = false;
					Settings.SaveSettings ();
				}
				catch
				{
				}
			}

			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);

			var m = new MainForm();
			m.Show();

			if (args.Length > 0)
			{
				Uri server = new Uri (args[0]);
				m.Connect (server.Host, (server.Port != -1) ? server.Port : 6112);
				Application.Run (m);
			}
			else if (m.ShowConnect (true))
				Application.Run (m);

			Settings.SaveSettings();
		}
	}
}