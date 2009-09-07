// Copyright (c) 2009, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

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
	public class ClientAudioSourceStateChangeMessage
		: ClientMessage
	{
		public ClientAudioSourceStateChangeMessage ()
			: base (ClientMessageType.ClientAudioSourceStateChange)
		{
		}

		public ClientAudioSourceStateChangeMessage (bool starting, int sourceId, int channelID)
			: base (ClientMessageType.ClientAudioSourceStateChange)
		{
			this.Starting = starting;
			this.SourceId = sourceId;
			this.ChannelId = channelID;
		}

		public bool Starting
		{
			get; set;
		}

		public int SourceId
		{
			get; set;
		}

		public int ChannelId
		{
			get; set;
		}

		#region Overrides of MessageBase

		public override void WritePayload(IValueWriter writer)
		{
			writer.WriteBool (this.Starting);
			writer.WriteInt32 (this.SourceId);
			writer.WriteInt32 (this.ChannelId);
		}

		public override void ReadPayload(IValueReader reader)
		{
			this.Starting = reader.ReadBool();
			this.SourceId = reader.ReadInt32();
			this.ChannelId = reader.ReadInt32();
		}

		#endregion
	}
}