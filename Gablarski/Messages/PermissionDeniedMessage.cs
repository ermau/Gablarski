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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	/// <summary>
	/// Generic permission denied message
	/// </summary>
	/// <remarks>
	/// Not all messages need an explicit result message,
	/// so this message will be used for any that don't but
	/// still need permissions.
	/// </remarks>
	public class PermissionDeniedMessage
		: ServerMessage
	{
		public PermissionDeniedMessage()
			: base (ServerMessageType.PermissionDenied)
		{
		}

		public PermissionDeniedMessage (ClientMessageType type)
			: this()
		{
			this.DeniedMessage = type;
		}

		public ClientMessageType DeniedMessage
		{
			get; set;
		}

		#region Overrides of MessageBase

		public override void WritePayload(IValueWriter writerm)
		{
			writerm.WriteUInt16 ((ushort)this.DeniedMessage);
		}

		public override void ReadPayload(IValueReader reader)
		{
			this.DeniedMessage = (ClientMessageType)reader.ReadUInt16();
		}

		#endregion
	}
}
