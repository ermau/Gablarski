// Copyright (c) 2010, Eric Maupin
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
using System.Linq;
using Gablarski.Messages;
using HttpServer;
using HttpServer.Sessions;
using Newtonsoft.Json;

namespace Gablarski.WebServer
{
	public class ChannelModule
		: SectionModule
	{
		public ChannelModule (ConnectionManager connectionManager)
			: base (connectionManager, "channels")
		{
		}

		protected override bool ProcessSection (IHttpRequest request, IHttpResponse response, IHttpSession session)
		{
			if (request.UriParts.Length == 1)
			{
				PermissionDeniedMessage denied;
				var msg = Connections.SendAndReceive<ChannelListMessage> (
					new RequestChannelListMessage(), session, out denied);

				if (denied != null)
				{
					WriteAndFlush (response, "{ \"error\": \"Permission denied\" }");
					return true;
				}

				WriteAndFlush (response, JsonConvert.SerializeObject (new { DefaultChannel = msg.DefaultChannelId, Channels = msg.Channels.RunQuery (request.QueryString) }));
			}
			else if (request.UriParts.Length == 2)
			{
				/*if (request.Method.ToLower() != "post")
					return false;*/

				IHttpInput input = (request.Method.ToLower() == "post") ? request.Form : request.QueryString;

				int channelId;
				switch (request.UriParts[1])
				{
					case "delete":
					case "edit":
					{
						if (!request.TryGetItemId (out channelId))
						{
							WriteAndFlush (response, "{ \"error\": \"Invalid channel ID\" }");
							return true;
						} 
						
						return SaveOrUpdateChannel (session, response, input, channelId, (request.UriParts[1] == "delete"));
					}
					
					case "new":
						return SaveOrUpdateChannel (session, response, input, 0, false);
				}
			}
			else
			{
				return false;
			}

			return true;
		}
		
		
		private bool SaveOrUpdateChannel (IHttpSession session, IHttpResponse response, IHttpInput input, int channelId, bool delete)
		{
			if (!input.ContainsAndNotNull ("SessionId", "ParentChannelId", "Name", "Description", "UserLimit") || session.Id != input["SessionId"].Value)
			{
				WriteAndFlush (response, "{ \"error\": \"Invalid request\" }");
				return true;
			}

			ChannelEditMessage editMessage;
			ChannelInfo channel = new ChannelInfo (channelId);

			if (!delete)
			{
				int userLimit, parentChannelId;
				if (!Int32.TryParse(input["ParentChannelId"].Value, out parentChannelId))
				{
					WriteAndFlush (response, "{ \"error\": \"Invalid request\" }");
					return true;
				}

				if (!Int32.TryParse(input["UserLimit"].Value, out userLimit))
				{
					WriteAndFlush (response, "{ \"error\": \"Invalid request\" }");
					return true;
				}

				channel.ParentChannelId = parentChannelId;
				channel.Name = input["Name"].Value.Trim();
				channel.Description = input["Description"].Value.Trim();
				channel.UserLimit = userLimit;

				bool defaultChannel;
				if (!Boolean.TryParse (input["Default"].Value, out defaultChannel))
				{
					WriteAndFlush (response, "{ \"error\": \"Invalid request\" }");
					return true;
				}

				editMessage = new ChannelEditMessage (channel) { MakeDefault = defaultChannel };
			}
			else
			{
				editMessage = new ChannelEditMessage (channel) { Delete = true };
			}

			var msg = Connections.SendAndReceive<ChannelEditResultMessage> (editMessage, session);
			WriteAndFlush (response, JsonConvert.SerializeObject (new { msg.ChannelId, msg.Result }));
			return false;
		}
	}
}