using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using Gablarski.Clients.Windows.Entities;
using System.Linq;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.Shell;

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

			log4net.Config.XmlConfigurator.Configure ();
			log4net.LogManager.GetLogger ("Gablarski WinForms").Info ("Program Start");

			if (Settings.FirstRun)
			{
				DialogResult result = MessageBox.Show ("Register gablarski:// urls with this client?", "Register gablarski://", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
				Settings.EnableGablarskiURLs = (result == DialogResult.OK);
				Settings.SaveSettings ();

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

			if (TaskbarManager.IsPlatformSupported)
			{
				var jl = JumpList.CreateJumpList ();
				var serverCategory = new JumpListCustomCategory ("Servers");

				string exe = Process.GetCurrentProcess ().MainModule.FileName;

				IList<ServerEntry> servers = Servers.GetEntries ().ToList ();
				JumpListLink[] links = new JumpListLink[servers.Count];
				for (int i = 0; i < servers.Count; ++i)
				{
					var s = servers[i];
					links[i] = new JumpListLink (exe, s.Name)
					{
						Arguments = s.Id.ToString(),
						IconReference = new IconReference (exe, 1)
					};
				}
				
				serverCategory.AddJumpListItems (links);

				jl.AddCustomCategories (serverCategory);

				try
				{
					jl.Refresh ();
				}
				catch (UnauthorizedAccessException) // Jumplists disabled
				{
				}
			}

			if (args.Length > 0)
			{
				int id;
				if (Int32.TryParse (args[0], out id))
				{
					ServerEntry server = Servers.GetEntries ().FirstOrDefault (s => s.Id == id);
					if (server == null)
					{
						if (m.ShowConnect (true))
							Application.Run (m);
					}
					else
					{
						m.Connect (server);
					}
				}
				else
				{
					Uri server = new Uri (args[0]);
					m.Connect (server.Host, (server.Port != -1) ? server.Port : 6112);
				}

				Application.Run (m);
			}
			else if (m.ShowConnect (true))
				Application.Run (m);

			Settings.SaveSettings();
		}
	}
}