﻿// Copyright (c) 2011, Eric Maupin
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
using System.Data;
using System.Linq;
using Gablarski.Server;
using NHibernate;
using NHibernate.Linq;

namespace Gablarski.LocalServer
{
	public class ChannelProvider
		: IChannelProvider
	{
		public event EventHandler ChannelsUpdated;

		public bool UpdateSupported
		{
			get { return true; }
		}

		public IChannelInfo DefaultChannel
		{
			get
			{
				using (ISession session = Persistance.SessionFactory.OpenSession())
					return GetDefaultChannel (session);
			}

			set
			{
				if (value == null)
					throw new ArgumentNullException ("value");

				using (ISession session = Persistance.SessionFactory.OpenSession())
				using (ITransaction transaction = session.BeginTransaction (IsolationLevel.Serializable))
				{
					var currentDefault = GetDefaultChannel (session);
					currentDefault.IsDefault = false;
					session.SaveOrUpdate (currentDefault);

					var newDefault = session.Load<LocalChannelInfo> (value.ChannelId);
					if (newDefault == null)
					{
						transaction.Rollback();
						return;
					}

					newDefault.IsDefault = true;
					session.SaveOrUpdate (newDefault);

					transaction.Commit();
				}

				OnChannelsUpdated (EventArgs.Empty);
			}
		}

		public IEnumerable<IChannelInfo> GetChannels()
		{
			using (ISession session = Persistance.SessionFactory.OpenSession())
				return session.Query<LocalChannelInfo>().Cast<IChannelInfo>().ToList();
		}

		public ChannelEditResult SaveChannel (IChannelInfo channel)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");

			using (ISession session = Persistance.SessionFactory.OpenSession())
			using (ITransaction transaction = session.BeginTransaction (IsolationLevel.Serializable))
			{
				var currentChannel = session.Get<LocalChannelInfo> (channel.ChannelId);
				if (currentChannel == null)
				{
					if (channel.ChannelId != 0)
						return ChannelEditResult.FailedChannelDoesntExist;
					
					currentChannel = new LocalChannelInfo();
				}

				currentChannel.Name = channel.Name;
				currentChannel.Description = channel.Description;
				currentChannel.ParentChannelId = channel.ParentChannelId;
				currentChannel.ReadOnly = channel.ReadOnly;
				currentChannel.UserLimit = channel.UserLimit;

				session.SaveOrUpdate (currentChannel);

				transaction.Commit();
			}

			OnChannelsUpdated (EventArgs.Empty);
			return ChannelEditResult.Success;
		}

		public ChannelEditResult DeleteChannel (IChannelInfo channel)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");

			using (ISession session = Persistance.SessionFactory.OpenSession())
			using (ITransaction transaction = session.BeginTransaction (IsolationLevel.Serializable))
			{
				var real = session.Get<LocalChannelInfo> (channel.ChannelId);
				if (real == null)
					return ChannelEditResult.FailedChannelDoesntExist;

				session.Delete (channel);
				transaction.Commit();
			}

			OnChannelsUpdated (EventArgs.Empty);
			return ChannelEditResult.Success;
		}

		public static void Setup (ISession session)
		{
			CreateLobby (session);
		}

		private static void CreateLobby (ISession session)
		{
			var lobby = new LocalChannelInfo
			{
				Name = "Lobby",
				IsDefault = true,
			};

			session.SaveOrUpdate (lobby);
		}

		private LocalChannelInfo GetDefaultChannel (ISession session)
		{
			var channel = session.Query<LocalChannelInfo>().SingleOrDefault (c => c.IsDefault);
			if (channel == null && !session.Query<LocalChannelInfo>().Any())
				CreateLobby (session);

			return channel;
		}

		private void OnChannelsUpdated (EventArgs e)
		{
			var updated = ChannelsUpdated;
			if (updated != null)
				updated (this, e);
		}
	}
}