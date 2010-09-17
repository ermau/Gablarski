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
using System.Linq;

namespace Gablarski
{
	public class BanInfo
		: IEquatable<BanInfo>
	{
		public BanInfo (string ipMask, string username, int length)
		{
			if (ipMask == null && username == null)
				throw new ArgumentNullException ("username", "Both ipMask and username can not be null");
			if (length < 0)
				throw new ArgumentOutOfRangeException("length", length, "Length must be >=0");

			IPMask = ipMask;
			Username = username;
			Length = length;
		}

		internal BanInfo (IValueReader reader)
		{
			Deserialize (reader);
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

		public virtual int Length
		{
			get;
			set;
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteString (IPMask);
			writer.WriteString (Username);
			writer.WriteInt32 (Length);
		}

		internal void Deserialize (IValueReader reader)
		{
			IPMask = reader.ReadString();
			Username = reader.ReadString();
			Length = reader.ReadInt32();
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
			return Equals (other.IPMask, IPMask) && Equals (other.Username, Username) && other.Length == Length;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (IPMask != null ? IPMask.GetHashCode() : 0);
				result = (result * 397) ^ (Username != null ? Username.GetHashCode() : 0);
				result = (result * 397) ^ Length;
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