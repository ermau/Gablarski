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

using System.Linq;
using Gablarski.Clients.Media;

namespace Gablarski.Clients
{
	/// <summary>
	/// Contract for notification receivers.
	/// </summary>
	public interface INotifier
		: INamedComponent
	{
		/// <summary>
		/// Sets media player integration for use with audio notifications.
		/// </summary>
		IMediaController Media { set; }

		/// <summary>
		/// Notifies the <see cref="INotifier"/> of a <paramref name="type"/> event.
		/// </summary>
		/// <param name="type">What happened?</param>
		/// <param name="contents">Notification contents.</param>
		/// <param name="priority">Priority of notification.</param>
		void Notify (NotificationType type, string contents, NotifyPriority priority);
	}

	/// <summary>
	/// Notification type.
	/// </summary>
	public enum NotificationType
	{
		/// <summary>
		/// You connected to a server.
		/// </summary>
		Connected,

		/// <summary>
		/// You disconnected from a server.
		/// </summary>
		Disconnected,

		/// <summary>
		/// Someone joined the server.
		/// </summary>
		UserJoinedServer,

		/// <summary>
		/// Someone left the server.
		/// </summary>
		UserLeftServer,

		/// <summary>
		/// You switched channels.
		/// </summary>
		SwitchedChannel,

		/// <summary>
		/// Someone joined your channel.
		/// </summary>
		UserJoinedChannel,

		/// <summary>
		/// Someone left your channel.
		/// </summary>
		UserLeftChannel,

		/// <summary>
		/// Someone received a boot to the head.
		/// </summary>
		UserKicked,

		/// <summary>
		/// Announcing the current song.
		/// </summary>
		Song
	}

	/// <summary>
	/// Notification priorities
	/// </summary>
	public enum NotifyPriority
	{
		/// <summary>
		/// General information.
		/// </summary>
		Info,

		/// <summary>
		/// Important information.
		/// </summary>
		Important
	}
}