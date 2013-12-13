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
using System.Linq;
using Tempest;

namespace Gablarski.Audio
{
	public class AudioSource
		: IEquatable<AudioSource>
	{
		internal AudioSource (ISerializationContext context, IValueReader reader)
		{
			Deserialize (context, reader);
		}

		internal AudioSource (AudioSource source)
			: this (source.Name, source.Id, source.OwnerId, source.IsMuted, source.CodecSettings)
		{
		}

		internal AudioSource (string name, int sourceId, int ownerId, AudioCodecArgs args)
			: this (name, sourceId, ownerId, false, args)
		{
		}

		internal AudioSource (string name, int sourceId, int ownerId, AudioFormat format, int bitrate, short frameSize, byte complexity)
			: this (name, sourceId, ownerId, format, bitrate, frameSize, complexity, false)
		{
		}

		internal AudioSource (string name, int sourceId, int ownerId, AudioFormat format, int bitrate, short frameSize, byte complexity, bool isMuted)
			: this (name, sourceId, ownerId, isMuted, new AudioCodecArgs (format, bitrate, frameSize, complexity))
		{
		}

		/// <summary>
		/// Constructs a new audio source.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="sourceId">If sourceId &lt; 0, the audio source is local.</param>
		public AudioSource (string name, int sourceId, int ownerId, bool isMuted, AudioCodecArgs args)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (args == null)
				throw new ArgumentNullException ("args");
			if (sourceId == 0)
				throw new ArgumentOutOfRangeException ("sourceId");
			if (sourceId > 0 && ownerId == 0)
				throw new ArgumentException ("ownerId");			

			Name = name;
			Id = sourceId;
			OwnerId = ownerId;
			IsMuted = isMuted;
			CodecSettings = args;
		}

		/// <summary>
		/// Gets if the source is muted by you or the server.
		/// </summary>
		public bool IsMuted
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the user-local name of the source.
		/// </summary>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the ID of the source.
		/// </summary>
		public int Id
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the owner's identifier.
		/// </summary>
		public int OwnerId
		{
			get;
			private set;
		}

		public AudioCodecArgs CodecSettings
		{
			get;
			private set;
		}

		public override string ToString ()
		{
			return "AudioSource:" + Name + ":" + Id + ":" + OwnerId;
		}

		public void Serialize (ISerializationContext context, IValueWriter writer)
		{
			writer.WriteString (Name);
			writer.WriteInt32 (Id);
			writer.WriteInt32 (OwnerId);
			writer.WriteBool (IsMuted);
			CodecSettings.Serialize (context, writer);
		}

		public void Deserialize (ISerializationContext context, IValueReader reader)
		{
			Name = reader.ReadString();
			Id = reader.ReadInt32 ();
			OwnerId = reader.ReadInt32();
			IsMuted = reader.ReadBool();
			CodecSettings = new AudioCodecArgs (context, reader);
		}

		public override bool Equals (object obj)
		{
			if (ReferenceEquals (null, obj))
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return Equals ((AudioSource) obj);
		}

		public bool Equals (AudioSource other)
		{
			if (ReferenceEquals (null, other))
				return false;
			if (ReferenceEquals (this, other))
				return true;
			return IsMuted.Equals (other.IsMuted) && string.Equals (Name, other.Name) && Id == other.Id && OwnerId == other.OwnerId && CodecSettings.Equals (other.CodecSettings);
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = IsMuted.GetHashCode();
				hashCode = (hashCode * 397) ^ Name.GetHashCode();
				hashCode = (hashCode * 397) ^ Id;
				hashCode = (hashCode * 397) ^ OwnerId;
				hashCode = (hashCode * 397) ^ CodecSettings.GetHashCode();
				return hashCode;
			}
		}

		internal void CopyTo (AudioSource targetSource)
		{
			targetSource.Name = Name;
			targetSource.Id = Id;
			targetSource.OwnerId = OwnerId;
			targetSource.IsMuted = IsMuted;
			targetSource.CodecSettings = new AudioCodecArgs (targetSource.CodecSettings);
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