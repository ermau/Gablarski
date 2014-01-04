// Copyright (c) 2009-2014, Eric Maupin
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

using Gablarski.Audio;
using Gablarski.Client;
using Tempest;

namespace Gablarski.Tests
{
	public class MockClientContext
		: TempestClient, IGablarskiClientContext
	{
		public MockClientContext (IClientConnection connection)
			: base (connection, MessageTypes.All, false)
		{
		}

		public IAudioEngine Audio { get; set; }

		/// <summary>
		/// Gets the channels in this context
		/// </summary>
		public IClientChannelHandler Channels { get; set; }

		/// <summary>
		/// Gets the source handler in this context
		/// </summary>
		public IClientSourceHandler Sources { get; set; }

		/// <summary>
		/// Gets the user associated with this context
		/// </summary>
		public IClientUserHandler Users { get; set; }

		/// <summary>
		/// Gets the current logged in user.
		/// </summary>
		public ICurrentUserHandler CurrentUser { get; set; }

		public ServerInfo ServerInfo { get; set; }
	}
}