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

		public ChannelInfo DefaultChannel
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
			}
		}

		public IEnumerable<ChannelInfo> GetChannels()
		{
			using (ISession session = Persistance.SessionFactory.OpenSession())
				return session.Linq<LocalChannelInfo>().Cast<ChannelInfo>().ToList();
		}

		public ChannelEditResult SaveChannel (ChannelInfo channel)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");

			using (ISession session = Persistance.SessionFactory.OpenSession())
			using (ITransaction transaction = session.BeginTransaction (IsolationLevel.Serializable))
			{
				var currentChannel = session.Load<LocalChannelInfo> (channel.ChannelId);
				if (currentChannel == null)
				{
					if (channel.ChannelId != 0)
						return ChannelEditResult.FailedChannelDoesntExist;
				}

				transaction.Commit();
				return ChannelEditResult.Success;
			}
		}

		public ChannelEditResult DeleteChannel (ChannelInfo channel)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");

			using (ISession session = Persistance.SessionFactory.OpenSession())
			using (ITransaction transaction = session.BeginTransaction (IsolationLevel.Serializable))
			{
				var real = session.Load<LocalChannelInfo> (channel.ChannelId);
				if (real == null)
					return ChannelEditResult.FailedChannelDoesntExist;

				session.Delete (channel);
				transaction.Commit();
				return ChannelEditResult.Success;
			}
		}

		private LocalChannelInfo GetDefaultChannel (ISession session)
		{
			return session.Linq<LocalChannelInfo>().SingleOrDefault (c => c.IsDefault);
		}
	}
}