// Copyright (c) 2009, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

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
using Gablarski.Server;

namespace Gablarski
{
	public class ServerInfo
	{
		internal ServerInfo (IValueReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			this.Deserialize (reader);
		}

		internal ServerInfo (ServerSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			this.ServerName = settings.Name;
			this.ServerDescription = settings.Description;
			this.ServerLogo = settings.ServerLogo;
		}

		/// <summary>
		/// Gets the name of the server.
		/// </summary>
		public string ServerName
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the server description.
		/// </summary>
		public string ServerDescription
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the url of the server's logo.
		/// </summary>
		public string ServerLogo
		{
			get;
			private set;
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteString (this.ServerName);
			writer.WriteString (this.ServerDescription);
			writer.WriteString (this.ServerLogo);
		}

		internal void Deserialize (IValueReader reader)
		{
			this.ServerName = reader.ReadString();
			this.ServerDescription = reader.ReadString();
			this.ServerLogo = reader.ReadString();
		}
	}
}