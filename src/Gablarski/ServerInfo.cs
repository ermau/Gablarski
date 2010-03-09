// Copyright (c) 2009, Eric Maupin
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
using Gablarski.Server;

namespace Gablarski
{
	public class ServerInfo
	{
		internal ServerInfo (IValueReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			Deserialize (reader);
		}

		internal ServerInfo (ServerSettings settings, IUserProvider userProvider)
		{
			if (settings == null)
				throw new ArgumentNullException ("settings");
			if (userProvider == null)
				throw new ArgumentNullException ("userProvider");

			this.Name = settings.Name;
			this.Description = settings.Description;
			this.Logo = settings.ServerLogo;
			this.Passworded = !String.IsNullOrEmpty (settings.ServerPassword);
			RegistrationMode = userProvider.RegistrationMode;
			
			if (userProvider.RegistrationMode != UserRegistrationMode.None)
				RegistrationContent = userProvider.RegistrationContent;
		}

		/// <summary>
		/// Gets the name of the server.
		/// </summary>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the server description.
		/// </summary>
		public string Description
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the url of the server's logo.
		/// </summary>
		public string Logo
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets whether the server is passworded or not.
		/// </summary>
		public bool Passworded
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Gets the server's registration mode. 
		/// </summary>
		public UserRegistrationMode RegistrationMode
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Gets the server's registration content. 
		/// </summary>
		public string RegistrationContent
		{
			get;
			private set;
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteString (Name);
			writer.WriteString (Description);
			writer.WriteString (Logo);
			writer.WriteBool (Passworded);
			writer.WriteByte ((byte)RegistrationMode);
			
			if (RegistrationMode != UserRegistrationMode.None)
				writer.WriteString (RegistrationContent);
		}

		internal void Deserialize (IValueReader reader)
		{
			Name = reader.ReadString();
			Description = reader.ReadString();
			Logo = reader.ReadString();
			Passworded = reader.ReadBool();
			RegistrationMode = (UserRegistrationMode)reader.ReadByte();
			
			if (RegistrationMode != UserRegistrationMode.None)
				RegistrationContent = reader.ReadString();
		}
	}
}