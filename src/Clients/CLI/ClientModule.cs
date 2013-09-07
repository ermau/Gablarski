// Copyright (c) 2011, Eric Maupin
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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Gablarski.Client;
using Tempest;

namespace Gablarski.Clients.CLI
{
	public class ClientModule
		: GClientModule
	{
		public ClientModule (GablarskiClient client, TextWriter writer)
			: base (client, writer)
		{
			client.Connected += OnConnected;
			client.CurrentUser.ReceivedJoinResult += OnJoinResult;
			client.CurrentUser.ReceivedLoginResult += OnLoginResult;
		}

		private void OnLoginResult (object sender, ReceivedLoginResultEventArgs e)
		{
			Writer.WriteLine ("Login result: {0}", e.Result.ResultState);
		}

		private void OnJoinResult (object sender, ReceivedJoinResultEventArgs e)
		{
			Writer.WriteLine ("Join result: {0}", e.Result);
		}

		void OnConnected (object sender, EventArgs e)
		{
			Writer.WriteLine ("Connected");
		}

		public override bool Process (string line)
		{
			var parts = CommandLine.Parse (line);
			if (parts[0] != "client")
				return false;

			if (parts.Count == 1)
			{
				Writer.WriteLine ("client commands:");
				Writer.WriteLine ("connect - connects to a server");
				Writer.WriteLine ("join - joins the server as a user");
				Writer.WriteLine ("disconnect - disconnects from the current server");
				return true;
			}

			switch (parts[1])
			{
				case "connect":
				{
					if (parts.Count < 3 || parts.Count == 4 || parts.Count > 5)
					{
						Writer.WriteLine ("client connect <host[:port]>");
						Writer.WriteLine ("client connect <host[:port]> <username> <password>");
						return true;
					}

					string[] hostParts = parts[2].Split (':');
					int port = (hostParts.Length == 1) ? 42912 : Int32.Parse (hostParts[1]);

					Client.ConnectAsync (new Target (hostParts[0], port));

					return true;
				}
				
				case "disconnect":
				{
					if (Client.IsConnected)
						Client.DisconnectAsync();
					else
						Writer.WriteLine ("Not connected");
					
					return true;
				}

				case "join":
				{
					if (parts.Count < 3 || parts.Count > 5)
					{
						Writer.WriteLine ("client join <nickname>");
						Writer.WriteLine ("client join <nickname> <phonetic>");
						Writer.WriteLine ("client join <nickname> <phonetic> <serverpassword>");
						return true;
					}

					if (parts.Count == 3)
						Client.CurrentUser.Join (parts[2], null);
					else if (parts.Count == 4)
						Client.CurrentUser.Join (parts[2], parts[3], null);
					else
						Client.CurrentUser.Join (parts[2], parts[3], parts[4]);

					return true;
				}

				default:
					return false;
			}
		}
	}
}