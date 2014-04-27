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
using Tempest;

namespace Gablarski.Messages
{
	public class ServerAudioDataMessage
		: GablarskiMessage
	{
		public ServerAudioDataMessage ()
			: base (GablarskiMessageType.ServerAudioData)
		{
		}

		public ServerAudioDataMessage (int sourceId, int sequence, byte frames, byte[][] data)
			: this()
		{
			#if DEBUG
			if (sourceId <= 0)
				throw new ArgumentOutOfRangeException ("sourceId");
			if (data == null)
				throw new ArgumentNullException ("data");
			if (frames <= 1)
				throw new ArgumentOutOfRangeException ("frames");
			#endif

			SourceId = sourceId;
			Sequence = sequence;
			Data = data;
		}

		public int SourceId
		{
			get;
			set;
		}

		public int Sequence
		{
		    get;
		    set;
		}

		public byte[][] Data
		{
			get;
			set;
		}

		public override bool MustBeReliable
		{
			get { return false; }
		}

		public override bool PreferReliable
		{
			get { return false; }
		}

		public override void WritePayload (ISerializationContext context, IValueWriter writer)
		{
			writer.WriteInt32 (SourceId);
			writer.WriteInt32 (Sequence);
			
			writer.WriteByte ((byte)Data.Length);
			for (int i = 0; i < Data.Length; ++i)
				writer.WriteBytes (Data[i]);
		}

		public override void ReadPayload (ISerializationContext context, IValueReader reader)
		{
			SourceId = reader.ReadInt32 ();
			Sequence = reader.ReadInt32();
			
			byte frames = reader.ReadByte();
			Data = new byte[frames][];

			for (int i = 0; i < frames; ++i)
				Data[i] = reader.ReadBytes();
		}
	}
}