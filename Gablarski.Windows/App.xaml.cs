// Copyright (c) 2012, Eric Maupin
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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using Gablarski.Client;
using Tempest;
using Tempest.Social;

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
			Client.ConnectSocialAsync (new Target (Target.LoopbackIP, SocialProtocol.DefaultPort))
				.ContinueWith (t =>
				{
					Client.BuddyList.Add (Client.Persona);
				});

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
