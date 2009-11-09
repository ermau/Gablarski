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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Clients.Input
{
	public enum InputState
	{
		On,
		Off
	}

	public class InputStateChangedEventArgs
		: EventArgs
	{
		public InputStateChangedEventArgs (InputState state)
		{
			this.State = state;
		}

		public InputState State
		{
			get; private set;
		}
	}

	public interface IInputProvider
		: IDisposable
	{
		/// <summary>
		/// Fired when the settings attached with <see cref="Attach"/> are matched.
		/// </summary>
		event EventHandler<InputStateChangedEventArgs> InputStateChanged;

		/// <summary>
		/// The displayable name of the input provider.
		/// </summary>
		string DisplayName { get; }

		/// <summary>
		/// Attaches the input provider to listen for the given settings.
		/// </summary>
		/// <param name="window">Application window handle.</param>
		/// <param name="settings">The settings provided by <see cref="EndRecord()"/>. <c>null</c> or <c>String.Empty</c> if not yet set.</param>
		/// <exception cref="ArgumentException">If <paramref name="window"/> is equal to <see cref="IntPtr.Zero"/>.</exception>
		/// <exception cref="InvalidOperationException">If it's already attached.</exception>
		void Attach (IntPtr window, string settings);

		/// <summary>
		/// Shuts down and detatches the input provider.
		/// </summary>
		void Detach();

		/// <summary>
		/// Starts recording input combinations for saving.
		/// </summary>
		/// <exception cref="InvalidOperationException">If called before <see cref="Attach"/>.</exception>
		void BeginRecord();

		/// <summary>
		/// Gets a nice display name for the given <paramref name="inputSettings"/>.
		/// </summary>
		/// <param name="inputSettings">The input settings to beautify.</param>
		/// <returns>The nice display name for <paramref name="inputSettings"/>.</returns>
		string GetNiceInputName (string inputSettings);

		/// <summary>
		/// Stops recording input combinations and returns the latest combination.
		/// </summary>
		/// <returns>The latest input combination, <c>null</c> if no input was gathered.</returns>
		string EndRecord();

		/// <summary>
		/// Stops recording input combinations and returns the latest combination.
		/// </summary>
		/// <param name="niceName">A human-readable string for for the input combination.</param>
		/// <returns>The latest input combination, <c>null</c> if not input was gathered.</returns>
		string EndRecord (out string niceName);
	}
}