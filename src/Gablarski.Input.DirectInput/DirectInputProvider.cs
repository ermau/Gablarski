// Copyright (c) 2011-2013, Eric Maupin
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
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Gablarski.Clients.Input;
using SharpDX.DirectInput;

namespace Gablarski.Input.DirectInput
{
	[Export (typeof (IInputProvider))]
	public class DirectInputProvider
		: IInputProvider
	{
		public event EventHandler<CommandStateChangedEventArgs> CommandStateChanged;
		public event EventHandler<RecordingEventArgs> NewRecording;

		public string Name
		{
			get { return "DirectInput"; }
		}

		public IEnumerable<CommandBinding> Defaults
		{
			get { yield return new CommandBinding (this, Command.Talk, String.Format ("{0}|{1}", "keyboard", Key.X.ToString())); }
		}

		public Task AttachAsync (IntPtr window)
		{
			return Task.Factory.StartNew (() => {
				this.input = new SharpDX.DirectInput.DirectInput();

				int index = 2;
				this.running = true;

				lock (this.syncRoot) {
					List<AutoResetEvent> resets = new List<AutoResetEvent>();
					foreach (DeviceInstance di in this.input.GetDevices (DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly)) {
						AutoResetEvent reset = new AutoResetEvent (false);

						var d = new Joystick (this.input, di.InstanceGuid);
						d.Properties.BufferSize = 10;
						d.SetCooperativeLevel (window, CooperativeLevel.Background | CooperativeLevel.NonExclusive);
						d.SetNotification (reset);
						d.Acquire();

						resets.Add (reset);
						this.joysticks.Add (di.InstanceGuid, d);
						this.joystickIndexes.Add (index++, di.InstanceGuid);
					}

					this.waits = new AutoResetEvent[this.joysticks.Count + 2];
					this.waits[0] = new AutoResetEvent (false);
					this.keyboard = new Keyboard (this.input);
					this.keyboard.Properties.BufferSize = 10;
					this.keyboard.SetCooperativeLevel (window, CooperativeLevel.Background | CooperativeLevel.NonExclusive);
					this.keyboard.SetNotification (this.waits[0]);
					this.keyboard.Acquire();

					this.waits[1] = new AutoResetEvent (false);
					this.mouse = new Mouse (this.input);
					this.mouse.Properties.BufferSize = 10;
					this.mouse.SetCooperativeLevel (window, CooperativeLevel.Background | CooperativeLevel.NonExclusive);
					this.mouse.SetNotification (this.waits[1]);
					this.mouse.Acquire();

					resets.CopyTo (this.waits, 2);
				}

				(this.inputRunnerThread = new Thread (InputRunner) {
					Name = "DirectInput Runner",
					IsBackground = true
				}).Start();
			});
		}

		public void SetBindings (IEnumerable<CommandBinding> bindings)
		{
			if (bindings == null)
				throw new ArgumentNullException ("bindings");

			lock (this.syncRoot)
			{
				foreach (CommandBinding binding in bindings)
				{
					string[] parts = binding.Input.Split ('|');
					if (parts[0] == "keyboard")
						parts[0] = KeyboardGuid.ToString();
					else if (parts[1] == "mouse")
						parts[0] = MouseGuid.ToString();

					Guid deviceGuid;
					if (!Guid.TryParse (parts[0], out deviceGuid))
						continue;

					if (deviceGuid == KeyboardGuid)
					{
						string[] keys = parts[1].Split ('+');
						Key[] boundKeys = new Key[keys.Length];

						for (int i = 0; i < boundKeys.Length; ++i)
							boundKeys[i] = (Key)Enum.Parse (typeof (Key), keys[i]);

						this.keyboardBindings.Add (boundKeys, binding.Command);
					}
					else if (deviceGuid == MouseGuid)
					{
						int button;
						if (!Int32.TryParse (parts[1], out button))
							continue;

						this.mouseBindings.Add (button, binding.Command);
					}
					else
					{
						Dictionary<int, Command> jbindings;
						if (!this.joystickBindings.TryGetValue (deviceGuid, out jbindings))
							this.joystickBindings[deviceGuid] = jbindings = new Dictionary<int, Command>();

						string[] bindingParts = parts[1].Split (';');
						jbindings.Add (Int32.Parse (bindingParts[0]), binding.Command);
					}
				}
			}
		}

		public void Detach()
		{
			if (!this.running)
				return;

			this.running = false;

			this.waits[0].Set(); // Keyboard wait always present, forces out of possible deadlock.
			this.inputRunnerThread.Join();
			this.inputRunnerThread = null;

			lock (this.syncRoot)
			{
				if (this.waits != null)
				{
					for (int i = 0; i < this.waits.Length; ++i)
						this.waits[i].Close();
				}

				this.mouse.Unacquire();
				this.mouse.Dispose();

				this.keyboard.Unacquire();
				this.keyboard.Dispose();

				foreach (Device d in joysticks.Values)
				{
					d.Unacquire();
					d.Dispose();
				}

				this.mouseBindings.Clear();
				this.keyboardBindings.Clear();
				this.joystickBindings.Clear();

				this.joysticks.Clear();
				this.joystickIndexes.Clear();
				this.waits = null;
			}
		}

		public void BeginRecord()
		{
			if (!this.running)
				throw new InvalidOperationException ("Not attached.");

			this.recording = true;
		}

		public string GetNiceInputName (Command command, string deviceInput)
		{
			if (deviceInput == null || deviceInput.Trim() == String.Empty)
				throw new ArgumentNullException ("deviceInput");
			if (!deviceInput.Contains ("|"))
				throw new FormatException ("Input was in an unrecognized format.");

			string[] parts = deviceInput.Split ('|');
			Guid deviceGuid = new Guid (parts[0]);

			Device device = null;
			if (deviceGuid == KeyboardGuid)
				device = this.keyboard;
			else if (deviceGuid == MouseGuid)
				device = this.mouse;
			else {
				Joystick joystick;
				if (this.joysticks.TryGetValue (deviceGuid, out joystick))
					device = joystick;
			}

			if (device == null)
				return "Unknown Device";
			
			return GetNiceInputName (parts[1], device);
		}

		public void EndRecord()
		{
			if (!this.recording)
				throw new InvalidOperationException ("Not recording");

			this.recording = false;
		}

		public void Dispose()
		{
			Detach();
		}

		private bool running;
		private bool recording;

		private readonly Dictionary<Guid, Joystick> joysticks = new Dictionary<Guid, Joystick>();
		private readonly Dictionary<int, Guid> joystickIndexes = new Dictionary<int, Guid>();
		private readonly Dictionary<Guid, Dictionary<int, Command>> joystickBindings = new Dictionary<Guid, Dictionary<int, Command>>();
		private readonly Dictionary<Key[], Command> keyboardBindings = new Dictionary<Key[], Command>();
		private readonly Dictionary<int, Command> mouseBindings = new Dictionary<int, Command>();

		private readonly object syncRoot = new object();

		private AutoResetEvent[] waits;

		private SharpDX.DirectInput.DirectInput input;

		private static readonly Guid KeyboardGuid = new Guid ("6f1d2b61-d5a0-11cf-bfc7-444553540000");
		private Keyboard keyboard;

		private static readonly Guid MouseGuid = new Guid ("6f1d2b60-d5a0-11cf-bfc7-444553540000");
		private Mouse mouse;

		private Thread inputRunnerThread;

		private string GetNiceInputName (string deviceInput, Device device)
		{
			switch (device.Information.Type)
			{
				case DeviceType.Keyboard:
					return deviceInput.Replace ("Menu", "Alt");

				case DeviceType.Mouse:
					return "Mouse " + (Int32.Parse(deviceInput) + 1);

				default:
					string[] parts = deviceInput.Split (';');

					string r = device.Properties.ProductName + " " +
					           device.Information.InstanceName;
					if (parts.Length == 2)
						r += parts[1];

					return r;
			}
		}

		private void OnInputStateChanged (CommandStateChangedEventArgs e)
		{
			EventHandler<CommandStateChangedEventArgs> handler = CommandStateChanged;
			if (handler != null)
				handler (this, e);
		}

		private void OnNewRecording (RecordingEventArgs e)
		{
			EventHandler<RecordingEventArgs> handler = NewRecording;
			if (handler != null)
				handler (this, e);
		}

		private void InputRunner()
		{
			Dictionary<Device, Dictionary<int, int>> objectInitial = new Dictionary<Device, Dictionary<int, int>>();
			Dictionary<Device, Dictionary<int, InputRange>> objectRanges = new Dictionary<Device, Dictionary<int, InputRange>>();
			Dictionary<Device, HashSet<int>> buttons = new Dictionary<Device, HashSet<int>>();

			Key[] keyValues = new Key[237];
			for (int i = 0; i < keyValues.Length; ++i)
				keyValues[i] = (Key)i;

			bool[] keyRecordedState = null;

			lock (this.syncRoot)
			{
				foreach (Device d in this.joysticks.Values)
				{
					objectInitial.Add (d, new Dictionary<int, int>());
					objectRanges.Add (d, new Dictionary<int, InputRange>());
					buttons.Add (d, new HashSet<int>());

					foreach (DeviceObjectInstance o in d.GetObjects(DeviceObjectTypeFlags.Button))
						buttons[d].Add (o.Offset);
				}
			}

			Dictionary<Key[], bool> keybindingStates = new Dictionary<Key[], bool>();
			Dictionary<int, bool> mousebindingStates = new Dictionary<int, bool>();

			while (this.running)
			{
				int waited = WaitHandle.WaitAny (this.waits);
				lock (this.syncRoot)
				{
					switch (waited)
					{
						case 0: // Keyboard
						{
							KeyboardState state = this.keyboard.GetCurrentState();

							if (!this.recording && this.keyboardBindings != null)
							{
								foreach (var kvp in this.keyboardBindings)
								{
									int match = 0;
									for (int i = 0; i < kvp.Key.Length; ++i)
									{
										if (state.IsPressed (kvp.Key[i]))
											match++;
									}

									bool nowState = (match == kvp.Key.Length);
									bool currentState;
									if (keybindingStates.TryGetValue (kvp.Key, out currentState))
									{
										if (nowState != currentState)
										{
											OnInputStateChanged (new CommandStateChangedEventArgs (kvp.Value, nowState));
											keybindingStates[kvp.Key] = nowState;
										}
									}
									else if (nowState)
									{
										OnInputStateChanged (new CommandStateChangedEventArgs (kvp.Value, true));
										keybindingStates[kvp.Key] = true;
									}
								}
							}
							else
							{
								bool[] currentState = new bool[keyValues.Length];
								for (int i = 0; i < keyValues.Length; ++i)
									currentState[i] = state.IsPressed (keyValues[i]);

								bool up = false;
								if (keyRecordedState != null)
								{
									for (int i = 0; i < keyRecordedState.Length; ++i)
									{
										if (!keyRecordedState[i] || currentState[i])
											continue;

										up = true;
										break;
									}
								}
								else
									keyRecordedState = currentState;

								if (up)
								{
									string currentRecording = KeyboardGuid + "|";
									for (int i = 0; i < keyValues.Length; ++i)
									{
										if (!keyRecordedState[i])
											continue;

										if (currentRecording != KeyboardGuid + "|")
											currentRecording += "+";

										currentRecording += keyValues[i].ToString();
									}

									keyRecordedState = null;
									OnNewRecording (new RecordingEventArgs (this, currentRecording));
								}
								else
									keyRecordedState = currentState;
							}

							break;
						}

						case 1: // Mouse
						{
							if (!this.recording && this.mouseBindings.Count == 0)
								continue;

							bool[] state = this.mouse.GetCurrentState().Buttons;
							
							if (!this.recording)
							{
								for (int i = 0; i < state.Length; ++i)
								{
									Command c;
									if (!this.mouseBindings.TryGetValue (i, out c))
										continue;

									bool newState = state[i];
									bool currentState;
									if (mousebindingStates.TryGetValue (i, out currentState))
									{
										if (currentState != newState)
										{
											OnInputStateChanged (new CommandStateChangedEventArgs (c, newState));
											mousebindingStates[i] = newState;
										}
									}
									else if (newState)
									{
										OnInputStateChanged (new CommandStateChangedEventArgs (c, true));
										mousebindingStates[i] = true;
									}
								}
							}
							else
							{

								for (int i = 0; i < state.Length; ++i)
								{
									if (!state[i])
										continue;

									OnNewRecording (new RecordingEventArgs (this, MouseGuid + "|" + i));
									break;
								}
							}

							break;
						}

						default:
							if (!this.recording && this.joystickBindings.Count == 0)
								continue;

							var d = this.joysticks[this.joystickIndexes[waited]];
							var currentButtons = buttons[d];
							var currentInitials = objectInitial[d];
							var currentRanges = objectRanges[d];

							JoystickUpdate[] updates = d.GetBufferedData();
							if (updates == null)
								continue;

							for (int i = 0; i < updates.Length; i++)
							{
								JoystickUpdate update = updates[i];
								if (this.recording)
								{
									int initial;
									if (!currentInitials.TryGetValue (update.RawOffset, out initial))
									{
										if (!currentButtons.Contains (update.RawOffset))
										{
											try
											{
												currentRanges.Add (update.RawOffset, d.Properties.Range);
												currentInitials[update.RawOffset] = update.Value;
											}
											catch (NotSupportedException)
											{
											}
										}
										else
											OnNewRecording (new RecordingEventArgs (this, String.Format ("{0}|{1}", d.Information.InstanceGuid, update.Offset)));
									}
									else
									{
										InputRange range = currentRanges[update.RawOffset];
										int delta = Math.Abs (initial - update.Value);
										if (((float)delta / (range.Maximum - range.Minimum)) > 0.25) // >25% change
											OnNewRecording (new RecordingEventArgs (this, String.Format ("{0}|{1};{2}", d.Information.InstanceGuid, update.Offset, ((initial > update.Value) ? "+" : "-"))));
									}
								}
								else
								{
									Dictionary<int, Command> binds;
									if (!this.joystickBindings.TryGetValue (d.Information.InstanceGuid, out binds) || binds.Count == 0)
										continue;
									 
									Command c;
									if (!binds.TryGetValue (update.RawOffset, out c))
										continue;

									if (currentButtons.Contains (update.RawOffset))
										OnInputStateChanged (new CommandStateChangedEventArgs (c, (update.Value == 128)));
									else
									{
										InputRange range = currentRanges[update.RawOffset];

										double value = (update.Value != -1) ? update.Value : range.Maximum;
										OnInputStateChanged (new CommandStateChangedEventArgs (c, (value / (range.Maximum - range.Minimum)) * 100));
									}
								}
							}

							break;
					}
				}
			}
		}
	}
}