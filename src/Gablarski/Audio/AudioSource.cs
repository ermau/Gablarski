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
using System.Linq;

namespace Gablarski.Audio
{
	public class AudioSource
		: AudioCodec, IEquatable<AudioSource>
	{
		internal AudioSource (IValueReader reader)
			: base (reader)
		{
		}

		internal AudioSource (AudioSource source)
			: this (source.Name, source.Id, source.OwnerId, source.IsMuted, source)
		{
		}

		internal AudioSource (string name, int sourceId, int ownerId, AudioCodecArgs args)
			: this (name, sourceId, ownerId, false, args)
		{
		}

		internal AudioSource (string name, int sourceId, int ownerId, byte channels, int bitrate, int frequency, short frameSize, byte complexity)
			: this(name, sourceId, ownerId, channels, bitrate, frequency, frameSize, complexity, false)
		{
		}

		internal AudioSource (string name, int sourceId, int ownerId, byte channels, int bitrate, int frequency, short frameSize, byte complexity, bool isMuted)
			: this (name, sourceId, ownerId, isMuted, new AudioCodecArgs (channels, bitrate, frequency, frameSize, complexity))
		{
		}

		internal AudioSource (string name, int sourceId, int ownerId, bool isMuted, AudioCodecArgs args)
			: base (args)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (ownerId == 0)
				throw new ArgumentException ("ownerId");			
			if (sourceId <= 0)
				throw new ArgumentOutOfRangeException ("sourceId");

			this.Name = name;
			this.Id = sourceId;
			this.OwnerId = ownerId;
			this.IsMuted = isMuted;
		}

		/// <summary>
		/// Gets if the source is muted by you or the server.
		/// </summary>
		public bool IsMuted
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// Gets the user-local name of the source.
		/// </summary>
		public string Name
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// Gets the ID of the source.
		/// </summary>
		public int Id
		{
			get;
			protected internal set;
		}

		/// <summary>
		/// Gets the owner's identifier.
		/// </summary>
		public int OwnerId
		{
			get;
			protected internal set;
		}

		public override string ToString ()
		{
			return "AudioSource:" + Name + ":" + Id + ":" + OwnerId;
		}

		protected internal override void Serialize (IValueWriter writer)
		{
			writer.WriteString (this.Name);
			writer.WriteInt32 (this.Id);
			writer.WriteInt32 (this.OwnerId);
			writer.WriteBool (this.IsMuted);
			base.Serialize (writer);
		}

		protected internal override void Deserialize (IValueReader reader)
		{
			this.Name = reader.ReadString();
			this.Id = reader.ReadInt32 ();
			this.OwnerId = reader.ReadInt32();
			this.IsMuted = reader.ReadBool();
			base.Deserialize (reader);
		}

		public override bool Equals (object obj)
		{
			if (ReferenceEquals (null, obj))
				return false;
			if (ReferenceEquals (this, obj))
				return true;

			return Equals (obj as AudioSource);
		}

		public bool Equals (AudioSource other)
		{
			if (ReferenceEquals (null, other))
				return false;
			if (ReferenceEquals (this, other))
				return true;

			return base.Equals (other) && other.Id == this.Id;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				{
					return (base.GetHashCode() * 397) ^ this.Id;
				}
			}
		}

		public static bool operator == (AudioSource left, AudioSource right)
		{
			return Equals (left, right);
		}

		public static bool operator != (AudioSource left, AudioSource right)
		{
			return !Equals (left, right);
		}
	}
}