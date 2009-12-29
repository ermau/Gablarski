using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Client;

namespace Gablarski.Clients.CLI
{
	public class SourceModule
		: GClientModule
	{
		public SourceModule (GablarskiClient client, TextWriter writer)
			: base (client, writer)
		{
			client.Sources.AudioSourceStarted += OnSourceStarted;
			client.Sources.AudioSourceStopped += OnSourceStopped;
		}

		public override bool Process(string line)
		{
			throw new NotImplementedException();
		}

		private void OnSourceStopped(object sender, AudioSourceEventArgs e)
		{
			var user = Client.Users[e.Source.OwnerId];
			if (user == null)
				return;

			Writer.WriteLine("{0} stopped talking.", user.Nickname);
		}

		private void OnSourceStarted(object sender, AudioSourceEventArgs e)
		{
			var user = Client.Users[e.Source.OwnerId];
			if (user == null)
				return;

			Writer.WriteLine("{0} started talking.", user);
		}
	}
}