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

namespace Gablarski
{
	public class PublicRSAParameters
		: IEquatable<PublicRSAParameters>
	{
		public byte[] Exponent;
		public byte[] Modulus;
		
		public bool Equals (PublicRSAParameters other)
		{
			if (other == null)
				return false;
			
			if ((Exponent == null && other.Exponent != null) || (Exponent != null && other.Exponent == null))
				return false;
			if ((Modulus == null && other.Modulus != null) || (Modulus != null && other.Modulus == null))
				return false;

			if (Exponent != null && other.Exponent != null)
			{
				if (Exponent.Length != other.Exponent.Length)
					return false;

				for (int i = 0; i < Exponent.Length; ++i)
				{
					if (Exponent[i] != other.Exponent[i])
						return false;
				}
			}

			if (Modulus != null && other.Modulus != null)
			{
				if (Modulus.Length != other.Modulus.Length)
					return false;

				for (int i = 0; i < Modulus.Length; ++i)
				{
					if (Modulus[i] != other.Modulus[i])
						return false;
				}
			}

			return true;
		}

		public override bool Equals (object obj)
		{
			if (ReferenceEquals (null, obj))
				return false;
			if (obj.GetType() != typeof (PublicRSAParameters))
				return false;
			return Equals ((PublicRSAParameters)obj);
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteBytes (Exponent);
			writer.WriteBytes (Modulus);
		}

		internal void Deserialize (IValueReader reader)
		{
			Exponent = reader.ReadBytes();
			Modulus = reader.ReadBytes();
		}
	}
}