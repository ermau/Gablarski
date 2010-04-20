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
	public class BinaryValueWriter
		: BinaryWriter
	{
		private readonly IValueWriter writer;

		public BinaryValueWriter (IValueWriter writer)
		{
			this.writer = writer;
		}

		/// <exception cref="NotSupportedException">Always.</exception>
		public override Stream BaseStream
		{
			get { throw new NotSupportedException(); }
		}

		/// <exception cref="NotSupportedException">Always.</exception>
		public override long Seek (int offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void Write (bool value)
		{
			this.writer.WriteBool (value);
		}
		
		public override void Write (byte value)
		{
			this.writer.WriteByte (value);
		}

		public override void Write(byte[] buffer)
		{
			this.writer.WriteBytes (buffer);
		}

		public override void Write (byte[] buffer, int index, int count)
		{
			byte[] a = new byte[count];
			Array.Copy (buffer, index, a, 0, count);

			Write (a);
		}

		public override void Write (char ch)
		{
			this.writer.WriteString (ch.ToString());
		}

		public override void Write (char[] chars)
		{
			this.writer.WriteString (new string (chars));
		}

		public override void Write (char[] chars, int index, int count)
		{
			this.writer.WriteString (new string (chars, index, count));
		}

		public override void Write (float value)
		{
			Write (BitConverter.GetBytes (value));
		}

		public override void Write (double value)
		{
			Write (BitConverter.GetBytes (value));
		}

		public override void Write (sbyte value)
		{
			this.writer.WriteSByte (value);
		}

		public override void Write (short value)
		{
			this.writer.WriteInt16 (value);
		}

		public override void Write (int value)
		{
			this.writer.WriteInt32 (value);
		}

		public override void Write (long value)
		{
			this.writer.WriteInt64 (value);
		}

		public override void Write (ushort value)
		{
			this.writer.WriteUInt16 (value);
		}

		public override void Write (uint value)
		{
			this.writer.WriteUInt32 (value);
		}

		public override void Write (ulong value)
		{
			this.writer.WriteUInt64 (value);
		}

		public override void Write (decimal value)
		{
			throw new NotImplementedException();
		}
		
		public override void Write (string value)
		{
			this.writer.WriteString (value);
		}

		public override void Flush()
		{
			this.writer.Flush();
		}

		public override void Close()
		{
			this.writer.Dispose();
		}

		protected override void Dispose (bool disposing)
		{
			this.writer.Dispose();
		}
	}
}