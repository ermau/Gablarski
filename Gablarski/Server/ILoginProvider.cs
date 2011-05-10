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
using System.Threading.Tasks;
using Tempest;

namespace Gablarski.Server
{
	public interface ILoginProvider
	{
		string Name { get; }

		string Logo { get; }

		IEnumerable<Parameter> Parameters { get; }

		/// <summary>
		/// Attempts to login given <paramref name="parameters"/>.
		/// </summary>
		/// <param name="parameters">The parameters to login with.</param>
		/// <exception cref="ArgumentNullException"><paramref name="parameters"/> is <c>null</c>.</exception>
		/// <returns>A <see cref="Task{T}"/> that will complete </returns>
		Task<bool> Login (IEnumerable<Parameter> parameters);
	}

	public class LoginProvider
		: ISerializable
	{
		public LoginProvider (IValueReader reader)
		{
			Deserialize (reader);
		}

		public LoginProvider (ILoginProvider provider)
		{
			Name = provider.Name;
		}

		public string Name
		{
			get;
			private set;
		}

		public string Logo
		{
			get;
			private set;
		}

		public IEnumerable<Parameter> Parameters
		{
			get;
			private set;
		}

		public void Serialize (IValueWriter writer)
		{
			writer.WriteString (Name);
			writer.WriteString (Logo);
			writer.WriteEnumerable (Parameters);
		}

		public void Deserialize (IValueReader reader)
		{
			Name = reader.ReadString();
			Logo = reader.ReadString();
			Parameters = reader.ReadEnumerable (r => new Parameter (r));
		}
	}
}