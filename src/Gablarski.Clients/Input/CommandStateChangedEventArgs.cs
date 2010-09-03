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

namespace Gablarski.Clients.Input
{
	public enum InputState
	{
		///<summary>
		/// Wax on.
		///</summary>
		On,
		
		/// <summary>
		/// Wax off.
		/// </summary>
		Off,

		/// <summary>
		/// Karate.
		/// </summary>
		Axis
	}

	/// <summary>
	/// Provides data for the <see cref="IInputProvider.CommandStateChanged"/> event.
	/// </summary>
	public class CommandStateChangedEventArgs
		: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CommandStateChangedEventArgs"/> class.
		/// </summary>
		/// <param name="command">The command that's state has changed.</param>
		/// <param name="state">The new state of the <paramref name="command"/>.</param>
		public CommandStateChangedEventArgs (Command command, InputState state)
		{
			if (state == InputState.Axis)
				throw new ArgumentException ("State can not be axis without percentage setting", "state");

			this.Command = command;
			this.State = state;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandStateChangedEventArgs"/> class.
		/// </summary>
		/// <param name="command">The command that's state has changed.</param>
		/// <param name="state">The new state of the <paramref name="command"/>.</param>
		/// <param name="axisPercent">The percentage of the axis position.</param>
		public CommandStateChangedEventArgs (Command command, InputState state, double axisPercent)
		{
			this.Command = command;
			this.State = state;
			this.AxisPercent = axisPercent;
		}

		/// <summary>
		/// Gets the command that state has changed for.
		/// </summary>
		public Command Command
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the input state for the <see cref="Command"/>.
		/// </summary>
		public InputState State
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the percentage of an axis.
		/// </summary>
		public double AxisPercent
		{
			get;
			private set;
		}
	}

	/// <summary>
	/// Provides data for the <see cref="IInputProvider.NewRecording"/> event.
	/// </summary>
	public class RecordingEventArgs
		: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RecordingEventArgs"/> class.
		/// </summary>
		/// <param name="recordedInput">The recorded input.</param>
		/// <param name="provider">The provider that recorded the input.</param>
		public RecordingEventArgs (IInputProvider provider, string recordedInput)
		{
			Provider = provider;
			RecordedInput = recordedInput;
		}

		/// <summary>
		/// Gets the provider that recorded the input.
		/// </summary>
		public IInputProvider Provider
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the <see cref="Provider"/>-recognized recorded input.
		/// </summary>
		public string RecordedInput
		{
			get;
			private set;
		}
	}
}