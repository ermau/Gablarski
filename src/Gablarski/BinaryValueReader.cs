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
using System.IO;
using System.Linq;

namespace Gablarski
{
	public class BinaryValueReader
		: BinaryReader
	{
		public BinaryValueReader (IValueReader reader)
			: base (new MemoryStream())
		{
			this.reader = reader;
		}

		/// <exception cref="NotSupportedException">Always.</exception>
		public override Stream BaseStream
		{
			get { throw new NotSupportedException(); }
		}

		public override bool ReadBoolean()
		{
			return reader.ReadBool();
		}

		/// <exception cref="NotSupportedException">Always.</exception>
		public override int Read()
		{
			throw new NotSupportedException();
		}

		public override int Read (byte[] buffer, int index, int count)
		{
			Array.Copy (this.reader.ReadBytes (count), 0, buffer, index, count);
			return count;
		}

		/// <exception cref="NotSupportedException">Always.</exception>
		public override int Read (char[] buffer, int index, int count)
		{
			throw new NotSupportedException();
		}

		/// <exception cref="NotSupportedException">Always.</exception>
		public override int PeekChar()
		{
			throw new NotSupportedException();
		}

		public override byte[] ReadBytes (int count)
		{
			return this.reader.ReadBytes (count);
		}

		public override byte ReadByte()
		{
			return this.reader.ReadByte();
		}

		public override char ReadChar()
		{
			return this.reader.ReadString().ToCharArray()[0];
		}

		public override char[] ReadChars (int count)
		{
			return this.reader.ReadString().ToCharArray();
		}

		public override decimal ReadDecimal()
		{
			throw new NotImplementedException();
		}

		public override sbyte ReadSByte()
		{
			return this.reader.ReadSByte();
		}

		public override short ReadInt16()
		{
			return this.reader.ReadInt16();
		}

		public override int ReadInt32()
		{
			return this.reader.ReadInt32();
		}

		public override long ReadInt64()
		{
			return this.reader.ReadInt64();
		}

		public override ushort ReadUInt16()
		{
			return this.reader.ReadUInt16();
		}

		public override uint ReadUInt32()
		{
			return this.reader.ReadUInt32();
		}

		public override ulong ReadUInt64()
		{
			return this.reader.ReadUInt64();
		}

		public override float ReadSingle()
		{
			return BitConverter.ToSingle (this.reader.ReadBytes (4), 0);
		}

		public override double ReadDouble()
		{
			return BitConverter.ToDouble (this.reader.ReadBytes (8), 0);
		}

		public override string ReadString()
		{
			return this.reader.ReadString();
		}

		public override void Close()
		{
			this.reader.Dispose();
		}

		private readonly IValueReader reader;
	}
}