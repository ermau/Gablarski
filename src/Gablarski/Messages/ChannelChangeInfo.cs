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
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class ChannelChangeInfo
	{
		public ChannelChangeInfo ()
		{
		}

		public ChannelChangeInfo (int targetUserId, int targetChannelId, int previousChannelId)
		{
			if (targetUserId == 0)
				throw new ArgumentException ("targetUserId");
			if (targetChannelId < 0)
				throw new ArgumentOutOfRangeException ("targetChannelId");
			if (previousChannelId < 0)
				throw new ArgumentOutOfRangeException ("previousChannelId");

			this.TargetUserId = targetUserId;
			this.TargetChannelId = targetChannelId;
			this.PreviousChannelId = previousChannelId;
		}

		public ChannelChangeInfo (int targetUserId, int targetChannelId, int previousChannelId, int requestingUserId)
			: this (targetUserId, targetChannelId, previousChannelId)
		{
			if (requestingUserId == 0)
				throw new ArgumentException ("requestingUserId");

			this.RequestingUserId = requestingUserId;
		}

		public ChannelChangeInfo (IValueReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			this.Deserialize (reader);
		}

		/// <summary>
		/// Gets the ID of the player who moved the target player.
		/// </summary>
		public int RequestingUserId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the ID of the player being moved.
		/// </summary>
		public int TargetUserId
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the ID of the channel the player is being moved to.
		/// </summary>
		public int TargetChannelId
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the ID of the channel the player is being moved from.
		/// </summary>
		public int PreviousChannelId
		{
			get;
			private set;
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteInt32 (this.RequestingUserId);
			writer.WriteInt32 (this.TargetUserId);
			writer.WriteInt32 (this.TargetChannelId);
			writer.WriteInt32 (this.PreviousChannelId);
		}

		internal void Deserialize (IValueReader reader)
		{
			this.RequestingUserId = reader.ReadInt32();
			this.TargetUserId = reader.ReadInt32();
			this.TargetChannelId = reader.ReadInt32();
			this.PreviousChannelId = reader.ReadInt32 ();
		}
	}
}
