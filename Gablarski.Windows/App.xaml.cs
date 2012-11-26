using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using Gablarski.Client;
using Tempest;

namespace Gablarski.Windows
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static GablarskiClient Client;

		protected override void OnStartup (StartupEventArgs e)
		{
			AppDomain.CurrentDomain.UnhandledException += (o, exe) =>
			{
				if (Debugger.IsAttached)
					Debugger.Break();

				MessageBox.Show (exe.ExceptionObject.ToString(), "Fatal unhandled error", MessageBoxButton.OK, MessageBoxImage.Error);
			};

			string roaming = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
			string gb = Path.Combine (roaming, "Gablarski");

			Directory.CreateDirectory (gb);

			IAsymmetricKey key = GetKey (Path.Combine (gb, "client.key"));

			Client = new GablarskiClient (key);

			base.OnStartup (e);
		}

		private static IAsymmetricKey GetKey (string path)
		{
			IAsymmetricKey key = null;
			if (!File.Exists (path))
			{
				RSACrypto crypto = new RSACrypto();
				key = crypto.ExportKey (true);
				using (FileStream stream = File.Create (path))
					key.Serialize (null, new StreamValueWriter (stream));
			}

			if (key == null)
			{
				using (FileStream stream = File.OpenRead (path))
					key = new RSAAsymmetricKey (null, new StreamValueReader (stream));
			}

			return key;
		}
	}
}
