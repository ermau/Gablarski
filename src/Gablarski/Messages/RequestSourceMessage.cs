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
	public class RequestSourceMessage
		: ClientMessage
	{
		public RequestSourceMessage ()
			: base (ClientMessageType.RequestSource)
		{
		}

		public RequestSourceMessage (string name, int channels, int targetBitrate, short frameSize)
			: this ()
		{
			if (name == null)
				throw new ArgumentNullException("name");
			if (channels < 1 || channels > 2)
				throw new ArgumentOutOfRangeException ("channels");
			if ((frameSize != 0 && frameSize < 64) || frameSize > 1024)
				throw new ArgumentOutOfRangeException ("frameSize");
			if ((frameSize % 64) != 0)
				throw new ArgumentException ("frameSize");

			this.Name = name;
			this.Channels = channels;
			this.TargetBitrate = targetBitrate;

			if (frameSize != 0)
				this.FrameSize = frameSize;
		}

		public string Name
		{
			get; set;
		}

		public int Channels
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the target bitrate to be requested (0 leaves it to the server.)
		/// </summary>
		public int TargetBitrate
		{
			get;
			set;
		}

		private short frameSize = 256;

		public short FrameSize
		{
			get { return this.frameSize; }
			set { this.frameSize = value; }
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteString (this.Name);
			writer.WriteByte ((byte)this.Channels);
			writer.WriteInt32 (this.TargetBitrate);
			writer.WriteInt16 (this.FrameSize);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Name = reader.ReadString();
			this.Channels = reader.ReadByte ();
			this.TargetBitrate = reader.ReadInt32();
			this.FrameSize = reader.ReadInt16();
		}
	}
}