// Copyright (c) 2011-2013, Eric Maupin
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
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace Gablarski.Clients
{
	public static class Extensions
	{
		public static string GetSimpleName (this Type self)
		{
			if (self == null)
				throw new NullReferenceException();

			return String.Format ("{0}, {1}", self.FullName, self.Assembly.GetName().Name);
		}

		public static DbCommand CreateCommand (this DbConnection self, string text)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
			if (text == null)
				throw new ArgumentNullException ("text");

			var cmd = self.CreateCommand();
			cmd.CommandText = text;
			return cmd;
		}

		public static void WriteBytes (this BinaryWriter self, byte[] data)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
			if (data == null)
				throw new ArgumentNullException ("data");

			self.Write (data.Length);
			self.Write (data);
		}

		public static byte[] ReadBytes (this BinaryReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			int len = reader.ReadInt32();
			return reader.ReadBytes (len);
		}

		public static IDictionary ToDictionary<TKey, TValue> (this IEnumerable<KeyValuePair<TKey, TValue>> self)
		{
			if (self == null)
				throw new ArgumentNullException ("self");

			return self.ToDictionary (kvp => kvp.Key, kvp => kvp.Value);
		}
	}
}