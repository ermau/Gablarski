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

namespace Gablarski.Clients.Input
{
	/// <summary>
	/// Contract for speech recognition -> commands.
	/// </summary>
	public interface ISpeechRecognizer
		: INamedComponent, IDisposable
	{
		/// <summary>
		/// Fired when a bound command state has changed.
		/// </summary>
		/// <seealso cref="IInputProvider.CommandStateChanged"/>
		event EventHandler<CommandStateChangedEventArgs> CommandStateChanged;

		ITextToSpeech TextToSpeech { set; }

		/// <summary>
		/// Opens the recognizer and prepares it for use.
		/// </summary>
		/// <returns><c>true</c> if </returns>
		Task OpenAsync();

		/// <summary>
		/// Closes the recognizer.
		/// </summary>
		void Close();

		/// <summary>
		/// Updates the recognizer with the channels and users so it can update detections.
		/// </summary>
		/// <param name="channels">The channels</param>
		/// <param name="users">The users</param>
		/// <exception cref="ArgumentNullException"><paramref name="channels"/> or <paramref name="users"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException">The recognizer is currently stopped.</exception>
		void Update (IEnumerable<IChannelInfo> channels, IEnumerable<IUserInfo> users);

		/// <summary>
		/// Starts attempting to recognize a command.
		/// </summary>
		void StartRecognizing();

		/// <summary>
		/// Stops attempting to recognize a command.
		/// </summary>
		void StopRecognizing();
	}
}