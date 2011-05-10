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
	public sealed class Parameter
		: ISerializable
	{
		public Parameter (Type valueType)
		{
			if (valueType == null)
				throw new ArgumentNullException ("valueType");

			ValueType = valueType;
		}

		public Parameter (IValueReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			Deserialize (reader);
		}

		public string Name
		{
			get;
			set;
		}

		public Type ValueType
		{
			get;
			private set;
		}

		public object Value
		{
			get { return this.value; }
			set
			{
				if (!ValueType.IsAssignableFrom (value.GetType()))
					throw new ArgumentException ("value");

				this.value = value;
			}
		}

		public void Serialize (IValueWriter writer)
		{
			writer.WriteString (Name);
			writer.Write (ValueType);
			writer.Write (Value);
		}

		public void Deserialize (IValueReader reader)
		{
			Name = reader.ReadString();
			ValueType = reader.Read<Type>();
			Value = reader.Read<object>();
		}

		private object value;
	}
}