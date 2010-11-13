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
using System.Linq;

namespace Gablarski.Server
{
	/// <summary>
	/// Contract for providers of channels
	/// </summary>
	public interface IChannelProvider
	{
		/// <summary>
		/// Fired when the channel list or default channel is updated.
		/// </summary>
		event EventHandler ChannelsUpdated;

		/// <summary>
		/// Gets whether or not clients can create/update/delete channels.
		/// </summary>
		bool UpdateSupported { get; }

		/// <summary>
		/// Gets or sets the default channel. <c>null</c> if none set or not supported.
		/// </summary>
		/// <remarks>
		/// If no default channel is set or supported, the first channel returned from GetChannels will be used.
		/// </remarks>
		IChannelInfo DefaultChannel { get; set; }

		/// <summary>
		/// Gets a listing channels from the underlying source.
		/// </summary>
		/// <returns>The listing of channels.</returns>
		IEnumerable<IChannelInfo> GetChannels ();

		/// <summary>
		/// Creates or updates the <paramref name="channel"/>.
		/// </summary>
		/// <param name="channel">The channel to create or update.</param>
		ChannelEditResult SaveChannel (IChannelInfo channel);

		/// <summary>
		/// Deletes the <paramref name="channel"/>.
		/// </summary>
		/// <param name="channel">The channel to delete.</param>
		ChannelEditResult DeleteChannel (IChannelInfo channel);
	}

	public static class ChannelProviderExtensions
	{
		/// <summary>
		/// Gets the default channel or the first channel if no default set.
		/// </summary>
		/// <param name="self">The <c>IChannelProvider</c> to retrieve the channels from.</param>
		/// <returns>The default channel, the first channel if no default set or <c>null</c> if no channels.</returns>
		public static IChannelInfo GetDefaultOrFirst (this IChannelProvider self)
		{
			return (self.DefaultChannel ?? self.GetChannels ().FirstOrDefault ());
		}
	}
}