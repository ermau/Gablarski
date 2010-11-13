// Copyright (c) 2009, Eric Maupin
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
	public class ChannelListMessage
		: ServerMessage
	{
		public ChannelListMessage ()
			: base (ServerMessageType.ChannelList)
		{

		}

		public ChannelListMessage (IEnumerable<IChannelInfo> channels, IChannelInfo defaultChannel)
			: this()
		{
			if (channels == null)
				throw new ArgumentNullException ("channels");
			if (defaultChannel == null)
				throw new ArgumentNullException ("defaultChannel");

			Channels = channels;
			DefaultChannelId = defaultChannel.ChannelId;
			Result = GenericResult.Success;
		}

		public ChannelListMessage (GenericResult result)
			: this ()
		{
			this.Result = result;
		}

		/// <summary>
		/// Gets or sets the result of the request.
		/// </summary>
		public GenericResult Result
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the default channel in the message, <c>null</c> if request failed.
		/// </summary>
		public int DefaultChannelId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the channels in the message, <c>null</c> if request failed.
		/// </summary>
		public IEnumerable<IChannelInfo> Channels
		{
			get { return this.channels.Cast<IChannelInfo>(); }
			set { this.channels = value.Select (c => new ChannelInfo (c)).ToList(); }
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteGenericResult (this.Result);
			if (this.Result != GenericResult.Success)
				return;

			writer.WriteInt32 (DefaultChannelId);

			writer.WriteInt32 (this.Channels.Count ());
			foreach (var c in this.channels)
				c.Serialize (writer);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Result = reader.ReadGenericResult ();
			if (this.Result != GenericResult.Success)
				return;

			DefaultChannelId = reader.ReadInt32();

			int nchannels = reader.ReadInt32();
			this.channels = new List<ChannelInfo> (nchannels);
			for (int i = 0; i < nchannels; ++i)
				this.channels.Add (new ChannelInfo (reader));
		}
		
		private List<ChannelInfo> channels;
	}
}