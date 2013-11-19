// Copyright (c) 2013, Eric Maupin
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
using System.Data.Odbc;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Gablarski.Clients
{
	public enum UpdateChannel
	{
		Nightly = 1
	}

	public sealed class Update
	{
		public Update (Version version, string installerUrl)
		{
			if (version == null)
				throw new ArgumentNullException ("version");
			if (installerUrl == null)
				throw new ArgumentNullException ("installerUrl");

			Version = version;
			InstallerUrl = installerUrl;
		}

		public Version Version
		{
			get;
			private set;
		}

		public string InstallerUrl
		{
			get;
			private set;
		}
	}

	public static class Updater
	{
		public static Task<Update> CheckAsync (UpdateChannel channel)
		{
			return CheckAsync (channel, CancellationToken.None);
		}

		public static async Task<Update> CheckAsync (UpdateChannel channel, CancellationToken cancelToken)
		{
			if (!Enum.IsDefined (typeof (UpdateChannel), channel))
				throw new ArgumentException ("channel is an invalid value for UpdateChannel", "channel");

			string versionUrl = "http://files.gablarski.org/" + channel.ToString().ToLower() + "_version.txt";

			HttpClient client = new HttpClient();
			HttpResponseMessage response = await client.GetAsync (versionUrl, cancelToken).ConfigureAwait (false);
			string content = await response.Content.ReadAsStringAsync().ConfigureAwait (false);

			return new Update (new Version (content), "http://files.gablarski.org/" + channel.ToString().ToLower() + ".exe");
		}

		public static Task<string> DownloadAsync (Update update, IProgress<int> progress)
		{
			return DownloadAsync (update, progress, CancellationToken.None);
		}

		public static Task<string> DownloadAsync (Update update, IProgress<int> progress, CancellationToken cancelToken)
		{
			if (update == null)
				throw new ArgumentNullException ("update");
			if (progress == null)
				throw new ArgumentNullException ("progress");

			TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

			string path = Path.Combine (Path.GetTempPath(), "GablarskiUpdate.exe");

			WebClient web = new WebClient();
			web.DownloadFileCompleted += (sender, args) => {
				if (args.Cancelled)
					tcs.SetCanceled();
				else if (args.Error != null)
					tcs.SetException (args.Error);
				else
					tcs.SetResult (path);
			};

			web.DownloadProgressChanged += (sender, args) => progress.Report (args.ProgressPercentage);
			web.DownloadFileAsync (new Uri (update.InstallerUrl), path);

			cancelToken.Register (web.CancelAsync);

			return tcs.Task;
		}
	}
}
