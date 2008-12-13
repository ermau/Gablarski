using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Gablarski.Client;

namespace GablarskiClient
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup (StartupEventArgs e)
		{
			if (!Debugger.IsAttached)
				AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			base.OnStartup (e);
		}

		static void CurrentDomain_UnhandledException (object sender, UnhandledExceptionEventArgs e)
		{
			var ex = (Exception) e.ExceptionObject;
			MessageBox.Show (ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace);

			PushToTalk.Uninstall();
		}

		protected override void OnExit (ExitEventArgs e)
		{
			PushToTalk.Uninstall();

			base.OnExit (e);
		}
	}
}
