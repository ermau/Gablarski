//
// DataResponder.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2014, Xamarin Inc.
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gablarski.Clients.Messages;
using Gablarski.Clients.Persistence;

namespace Gablarski.Clients
{
	public class DataResponder
	{
		public void BeginResponding (ServerEntry serverEntry)
		{
			if (serverEntry == null)
				throw new ArgumentNullException ("serverEntry");

			this.volumeTask = ClientData.GetVolumesAsync (serverEntry);
			this.ignoreTask = ClientData.GetIgnoresAsync (serverEntry);

			this.server = serverEntry;
			Messenger.Register<GetUserGainMessage> (OnUserVolumeMessageMessage);
			Messenger.Register<IgnoreUserMessage> (OnIgnoreUserMessage);
			Messenger.Register<AdjustUserGainMessage> (OnAdjustUserVolumeMessage);
		}

		public void StopResponding()
		{
			Messenger.Unregister<GetUserGainMessage> (OnUserVolumeMessageMessage);
			Messenger.Unregister<IgnoreUserMessage> (OnIgnoreUserMessage);
			Messenger.Unregister<AdjustUserGainMessage> (OnAdjustUserVolumeMessage);

			lock (this.sync) {
				this.volumeTask = null;
				this.ignoreTask = null;

				this.server = null;
				this.volumes = null;
			}
		}

		private ServerEntry server;
		
		private Task<IEnumerable<VolumeEntry>> volumeTask;
		private Task<IEnumerable<IgnoreEntry>> ignoreTask;

		private List<VolumeEntry> volumes;
		private List<IgnoreEntry> ignores;

		private readonly object sync = new object();

		private VolumeEntry GetVolumeEntry (string username)
		{
			var tasks = Interlocked.Exchange (ref this.volumeTask, null);

			VolumeEntry entry;
			lock (this.sync) {
				if (tasks != null)
					this.volumes = tasks.Result.ToList();

				entry = this.volumes.FirstOrDefault (v => v.Username == username);
			}

			return entry;
		}

		private IgnoreEntry GetIgnoreEntry (string username)
		{
			var task = Interlocked.Exchange (ref this.ignoreTask, null);

			IgnoreEntry entry;
			lock (this.sync) {
				if (task != null)
					this.ignores = task.Result.ToList();

				entry = this.ignores.FirstOrDefault (v => v.Username == username);
			}

			return entry;
		}

		private void OnAdjustUserVolumeMessage (AdjustUserGainMessage msg)
		{
			VolumeEntry entry = GetVolumeEntry (msg.User.Username);

			lock (this.sync) {
				if (entry == null) {
					entry = new VolumeEntry { ServerId = this.server.Id, Username = msg.User.Username };
				} else if (Math.Round (msg.Gain, 2) == 1) {
					this.volumes.Remove (entry);
					ClientData.Delete (entry);
					return;
				}

				entry.Gain = (float)msg.Gain;
				ClientData.SaveOrUpdate (entry);

				if (entry.VolumeId == 0)
					this.volumeTask = ClientData.GetVolumesAsync (this.server);
			}
		}

		private void OnUserVolumeMessageMessage (GetUserGainMessage message)
		{
			VolumeEntry entry = GetVolumeEntry (message.User.Username);
			if (entry != null)
				message.Gain = entry.Gain;
		}

		private void OnIgnoreUserMessage (IgnoreUserMessage msg)
		{
			IgnoreEntry entry = GetIgnoreEntry (msg.User.Username);
			if (msg.Ignore) {
				if (entry == null) {
					entry = new IgnoreEntry (0) { ServerId = this.server.Id, Username = msg.User.Username };
				}

				ClientData.SaveOrUpdate (entry);

				this.ignoreTask = ClientData.GetIgnoresAsync (this.server);
			} else if (entry != null) {
				ClientData.Delete (entry);
				this.ignores.Remove (entry);
			}
		}
	}
}
