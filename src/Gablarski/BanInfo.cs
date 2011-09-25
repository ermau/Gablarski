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
using Tempest;

namespace Gablarski
{
	public class BanInfo
		: ISerializable, IEquatable<BanInfo>
	{
		public BanInfo (string ipMask, string username, TimeSpan length)
		{
			if (ipMask == null && username == null)
				throw new ArgumentNullException ("username", "Both ipMask and username can not be null");

			IPMask = ipMask;
			Username = username;
			Length = length;
			Created = DateTime.Now;
		}

		protected BanInfo()
		{
		}

		internal BanInfo (ISerializationContext context, IValueReader reader)
		{
			Deserialize (context, reader);
		}

		public bool IsExpired
		{
			get { return !(this.Length == TimeSpan.Zero) && (this.Created.Add (this.Length) < DateTime.Now); }
		}

		public virtual string IPMask
		{
			get;
			set;
		}

		public virtual string Username
		{
			get;
			set;
		}

		public virtual DateTime Created
		{
			get;
			set;
		}

		public virtual TimeSpan Length
		{
			get;
			set;
		}

		public void Serialize (ISerializationContext context, IValueWriter writer)
		{
			writer.WriteString (IPMask);
			writer.WriteString (Username);
			writer.WriteInt64 (Created.ToBinary());
			writer.WriteInt64 (Length.Ticks);
		}

		public void Deserialize (ISerializationContext context, IValueReader reader)
		{
			IPMask = reader.ReadString();
			Username = reader.ReadString();
			Created = DateTime.FromBinary (reader.ReadInt64());
			Length = TimeSpan.FromTicks (reader.ReadInt64());
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals (null, obj))
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType() != typeof (BanInfo))
				return false;
			return Equals ((BanInfo)obj);
		}

		public bool Equals (BanInfo other)
		{
			if (ReferenceEquals (null, other))
				return false;
			if (ReferenceEquals (this, other))
				return true;
			return Equals (other.IPMask, IPMask) && Equals (other.Username, Username) && other.Created.Equals (Created) && other.Length.Equals (Length);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (IPMask != null ? IPMask.GetHashCode() : 0);
				result = (result * 397) ^ (Username != null ? Username.GetHashCode() : 0);
				result = (result * 397) ^ Created.GetHashCode();
				result = (result * 397) ^ Length.GetHashCode();
				return result;
			}
		}

		public static bool operator == (BanInfo left, BanInfo right)
		{
			return Equals (left, right);
		}

		public static bool operator != (BanInfo left, BanInfo right)
		{
			return !Equals (left, right);
		}
	}
}