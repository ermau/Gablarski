﻿// Copyright (c) 2011, Eric Maupin
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
using Tempest;

namespace Gablarski.Messages
{
	public enum DisconnectionReason
		: byte
	{
		Unknown = 0,

		/// <summary>
		/// Disconnected because the user logged in elsewhere.
		/// </summary>
		/// <remarks>
		/// Do not reconnect.
		/// </remarks>
		LoggedInElsewhere = 1,

		/// <summary>
		/// Disconnected because the user requested it.
		/// </summary>
		/// <remarks>
		/// Do not reconnect.
		/// </remarks>
		Requested = 2,

		/// <summary>
		/// Disconnected because you were impolitely asked to leave.
		/// </summary>
		/// <remarks>
		/// Do not reconnect.
		/// </remarks>
		Kicked = 3,
		
		/// <summary>
		/// Disconnected because your connection was rejected. (Local reason)
		/// </summary>
		Rejected = 4,

		/// <summary>
		/// Disconnected because you are redirecting. (Local reason)
		/// </summary>
		Redirected = 5
	}

	public class DisconnectMessage
		: GablarskiMessage
	{
		public DisconnectMessage ()
			: base (GablarskiMessageType.Disconnect)
		{
		}

		public DisconnectMessage (DisconnectionReason reason)
			: this()
		{
			Reason = reason;
		}

		public DisconnectionReason Reason
		{
			get;
			set;
		}

		public override void WritePayload (ISerializationContext context, IValueWriter writer)
		{
			writer.WriteByte ((byte)Reason);
		}

		public override void ReadPayload (ISerializationContext context, IValueReader reader)
		{
			Reason = (DisconnectionReason)reader.ReadByte ();
		}
	}
}