//
// AdjustUserVolumeMessage.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2014, Xamarin Inc.
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

namespace Gablarski.Clients.Messages
{
	public class AdjustUserVolumeMessage
		: UserMessage
	{
		/// <summary>
		/// Constructs and initializes a new instance of the <see cref="AdjustUserVolumeMessage" /> class.
		/// </summary>
		/// <param name="user">The user to adjust the volume of.</param>
		/// <param name="volume">The volume multiplier (1 for 100%, 1.5 for 150%).</param>
		public AdjustUserVolumeMessage (IUserInfo user, double volume)
			: base (user)
		{
			Volume = volume;
		}


		/// <summary>
		/// Gets the volume multiplier.
		/// </summary>
		public double Volume
		{
			get;
			private set;
		}
	}
}