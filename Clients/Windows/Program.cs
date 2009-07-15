using System;
using System.Windows.Forms;
using Kennedy.ManagedHooks;
using Microsoft.WindowsAPICodePack;

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
				/*MHook.UninstallHook();*/
				TaskDialog.Show ((e.ExceptionObject as Exception).ToDisplayString(), "Unexpected Error", "Unexpected Error",
				                 TaskDialogStandardIcon.Error);
			};

			KHook.InstallHook();
			//MHook.InstallHook();

			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);

			try
			{
				var login = new LoginForm();
				if (login.ShowDialog() == DialogResult.OK)
					Application.Run (new MainForm (login.Entry));
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