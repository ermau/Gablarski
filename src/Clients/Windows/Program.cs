// Copyright (c) 2010-2013, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using log4net;
using Microsoft.WindowsAPICodePack.Dialogs;
using Mindscape.Raygun4Net;
using System.Windows.Media;
using Elysium;
using Tempest;
using Tempest.Social;
using Gablarski.Clients.Input;
using Gablarski.Clients.Persistence;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Taskbar;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Gablarski.Clients.Windows
{
	static class Program
	{
		private const string RaygunKey = "{raygun}";
		public static RaygunClient Raygun;

		public static Task<RSAAsymmetricKey> Key;

		public static GablarskiSocialClient SocialClient;
		public static ChatHistory History;

		public static ILog StartupLog;

		public static void EnableGablarskiURIs()
		{
			try {
				Process p = new Process {
					StartInfo = new ProcessStartInfo ("cmd.exe", "/C ftype gablarski=\"" + Path.Combine (Environment.CurrentDirectory, Process.GetCurrentProcess ().ProcessName) + ".exe\" \"%1\"") {
						Verb = "runas",
					}
				};
				p.Start ();
				p.WaitForExit ();
			} catch (Win32Exception) {
			}
		}

		public static void DisableGablarskiURIs()
		{
			try {
				Process p = new Process {
					StartInfo = new ProcessStartInfo ("cmd.exe", "/C ftype gablarski=") {
						Verb = "runas",
					}
				};
				p.Start();
				p.WaitForExit();
			} catch (Win32Exception) {
			}
		}

		public static void UpdateTaskbarServers()
		{
			if (!TaskbarManager.IsPlatformSupported)
				return;

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

			try {
				jl.Refresh ();
			} catch (UnauthorizedAccessException) { // Jumplists disabled
			}
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main (string[] args)
		{
			if (RaygunKey[0] != '{')
				Raygun = new RaygunClient (RaygunKey);

			AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
				if (StartupLog != null)
					StartupLog.Error ("Fatal Error", (Exception)e.ExceptionObject);

				if (Raygun != null)
					Raygun.Send ((Exception) e.ExceptionObject, null, Settings.CurrentSettings.ToDictionary());

				MessageBox.Show ("Unexpected error" + Environment.NewLine + (e.ExceptionObject as Exception).ToDisplayString(),
				                 "Unexpected error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			};

			//AutomaticErrorReporter errors = new AutomaticErrorReporter();
			//errors.Add (new GablarskiErrorReporter (typeof(Program).Assembly));

			log4net.Config.XmlConfigurator.Configure ();
			StartupLog = LogManager.GetLogger ("Startup");
			StartupLog.DebugFormat ("PID: {0}", Process.GetCurrentProcess().Id);

			FileInfo program = new FileInfo (Process.GetCurrentProcess().MainModule.FileName);
			Environment.CurrentDirectory = program.Directory.FullName;

			string useLocalValue = ConfigurationManager.AppSettings["useLocalDatabase"];
			bool useLocal;
			Boolean.TryParse (useLocalValue, out useLocal);

			StartupLog.DebugFormat ("Local files: {0}", useLocal);
			StartupLog.DebugFormat ("Setting up databases..");

			Task dataSetup = ClientData.SetupAsync (useLocal);

			Task loadSettings = dataSetup.ContinueWith (async t => {
				await Settings.LoadAsync().ConfigureAwait (false);
				await ClientData.CheckForUpdatesAsync().ConfigureAwait (false);
			}).Unwrap();

			StartupLog.DebugFormat ("Databases setup.");

			if (!Debugger.IsAttached)
				CheckForUpdates();

			StartupLog.Debug ("Starting key retrieval");
			var keyCancelSource = new CancellationTokenSource();
			Key = dataSetup.ContinueWith (t => ClientData.GetCryptoKeyAsync (keyCancelSource.Token), keyCancelSource.Token).Unwrap();
			Key.ContinueWith (t => StartupLog.DebugFormat ("Key retrieval: {0}{1}{2}", t.Status, Environment.NewLine, t.Exception), keyCancelSource.Token);

			loadSettings.Wait();

			SetupFirstRun();

			System.Windows.Application app = new System.Windows.Application();
			app.Resources.MergedDictionaries.Add (new ResourceDictionary { Source = new Uri ("/Elysium;component/Themes/Generic.xaml", UriKind.Relative) });
			app.Resources.MergedDictionaries.Add (new ResourceDictionary { Source = new Uri ("Resources.xaml", UriKind.Relative) });
			app.Apply (Theme.Dark, AccentBrushes.Blue, Brushes.White);

			/*
			if (Settings.Nickname == null)
				PersonalSetup();*/

			if (!ShowKeyProgress (keyCancelSource))
				return;

			ResourceWebRequestFactory.Register();

			//Application.EnableVisualStyles ();
			//Application.SetCompatibleTextRenderingDefault (false);

			SetupSocial();

			var window = new MainWindow();
			window.Show();

			UpdateTaskbarServers();

			/*if (args.Length > 0) {
				int id;
				if (Int32.TryParse (args[0], out id)) {
					ServerEntry server = Servers.GetEntries ().FirstOrDefault (s => s.Id == id);
					if (server == null) {
						if (!m.ShowConnect (true))
							return;
					} else
						m.Connect (server);
				} else {
					Uri server = new Uri (args[0]);
					m.Connect (server.Host, (server.Port != -1) ? server.Port : 42912);
				}
			} else if (Settings.ShowConnectOnStart) {
				if (!m.ShowConnect (true))
					return;
			}*/

			app.Run (window);

			Settings.SaveAsync().Wait();
		}

		private static void CheckForUpdates()
		{
			StartupLog.Debug ("Checking for updates..");

			Updater.CheckAsync (UpdateChannel.Nightly).ContinueWith (t => {
				if (t.IsCanceled) {
					StartupLog.Debug ("Update check was canceled.");
					return;
				} else if (t.IsFaulted) {
					StartupLog.Debug ("Update check failed", t.Exception.Flatten());
					return;
				}

				Version currentVersion = typeof (Program).Assembly.GetName().Version;
				StartupLog.DebugFormat ("Update check: Current {0} vs. {1}", currentVersion, t.Result.Version);

				if (t.Result.Version <= currentVersion)
					return;

				TaskDialog update = new TaskDialog {
					InstructionText = "Would you like to update?",
					Text = String.Format ("You're using version {0}, version {1} is available.", currentVersion, t.Result.Version),
					Icon = TaskDialogStandardIcon.Information,
					Caption = "Gablarski",
					StartupLocation = TaskDialogStartupLocation.CenterScreen,
					StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No
				};

				update.Closing += (sender, args) => {
					if (args.TaskDialogResult != TaskDialogResult.Yes)
						return;

					Task.Run (() => {
						var cancelSource = new CancellationTokenSource();

						TaskDialog downloadUpdate = new TaskDialog {
							InstructionText = "Downloading update...",
							StandardButtons = TaskDialogStandardButtons.Cancel,
							ProgressBar = new TaskDialogProgressBar (0, 100, 0)
						};

						var progress = new Progress<int> (p => {
							downloadUpdate.ProgressBar.Value = p;
						});

						Updater.DownloadAsync (t.Result, progress, cancelSource.Token).ContinueWith (dt => {
							downloadUpdate.Close (TaskDialogResult.Ok);

							Process.Start (dt.Result);
							Environment.Exit (0);
						}, TaskContinuationOptions.OnlyOnRanToCompletion);

						if (downloadUpdate.Show() == TaskDialogResult.Cancel)
							cancelSource.Cancel();
					});
				};

				update.Show();
			});
		}

		private static bool ShowKeyProgress (CancellationTokenSource keyCancelSource)
		{
			if (Key.IsCompleted)
				return true;

			StartupLog.Debug ("Key retrieval not complete in time, showing dialog.");

			bool showing = false;
			TaskDialog progress = new TaskDialog {
				Caption = "Gablarski",
				InstructionText = "Setting up personal key...",
				ProgressBar = new TaskDialogProgressBar { State = TaskDialogProgressBarState.Marquee },
				StandardButtons = TaskDialogStandardButtons.Cancel,
				StartupLocation = TaskDialogStartupLocation.CenterScreen
			};

			progress.Opened += (s, e) => showing = true;

			Key.ContinueWith (t => {
				while (!showing)
					Thread.Sleep (1);

				progress.Close (TaskDialogResult.Ok);
			}, TaskContinuationOptions.NotOnCanceled);

			if (progress.Show() == TaskDialogResult.Cancel) {
				keyCancelSource.Cancel();
				return false;
			}

			return true;
		}

		private static void SetupFirstRun()
		{
			if (!Settings.FirstRun)
				return;

			StartupLog.Debug ("Setting up first run..");

			DialogResult result = MessageBox.Show ("Register gablarski:// urls with this client?", "Register gablarski://",
				MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

			Settings.EnableGablarskiUrls = (result == DialogResult.OK);
			Settings.Version = typeof (Program).Assembly.GetName().Version.ToString();
			Settings.SaveAsync().Wait();

			IInputProvider input = Modules.Input.FirstOrDefault (p => p.GetType().GetSimpleName() == Settings.InputProvider);
			if (input != null) {
				foreach (CommandBinding binding in input.Defaults)
					Settings.CommandBindings.Add (new CommandBinding (binding.Provider, binding.Command, binding.Input));
			}

			try {
				if (Settings.EnableGablarskiUrls)
					EnableGablarskiURIs();

				Settings.FirstRun = false;
				Settings.SaveAsync().Wait();
			} catch {
			}
		}

		private static void SetupSocial()
		{
			var person = new Person (Key.Result.PublicSignature.Aggregate (String.Empty, (s, b) => s + b.ToString ("X2"))) {
				Nickname = Settings.Nickname,
				Avatar = Settings.Avatar
			};

			SocialClient = new GablarskiSocialClient (person, Key.Result);

			History = new ChatHistory (SocialClient);

			string host = ConfigurationManager.AppSettings["socialHost"];
			//SocialClient.SetTarget (new Target (host, SocialProtocol.DefaultPort));
		}

		private static void PersonalSetup()
		{
			var setupWindow = new PersonaSetupWindow();
			bool? result = setupWindow.ShowDialog();
			if (!result.HasValue || !result.Value)
				Environment.Exit (0);
		}
	}
}