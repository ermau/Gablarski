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

using System.Threading.Tasks;
using Gablarski.Audio;
using Tempest;

namespace Gablarski.Client
{
	/// <summary>
	/// Represents a Gablarski client context.
	/// </summary>
	public interface IGablarskiClientContext
		: IClientContext
	{
		IAudioEngine Audio { get; }

		/// <summary>
		/// Information about the current server. (<c>null</c> if not connected)
		/// </summary>
		ServerInfo ServerInfo { get; }

		/// <summary>
		/// Gets the channels handler associated with this context.
		/// </summary>
		IClientChannelHandler Channels { get; }

		/// <summary>
		/// Gets the source handler associated with this context.
		/// </summary>
		IClientSourceHandler Sources { get; }

		/// <summary>
		/// Gets the user handler associated with this context.
		/// </summary>
		IClientUserHandler Users { get; }

		/// <summary>
		/// Gets the current logged in user.
		/// </summary>
		ICurrentUserHandler CurrentUser { get; }

		Task<ClientConnectionResult> ConnectAsync (Target target);
	}

	public static class ContextExtensions
	{
		public static IChannelInfo GetCurrentChannel (this IGablarskiClientContext self)
		{
			return self.Channels[self.CurrentUser.CurrentChannelId];
		}
	}
}