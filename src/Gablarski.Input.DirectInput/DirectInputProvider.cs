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
using System.Text;
using System.Threading;
using Cadenza.Collections;
using Gablarski.Clients.Input;
using Microsoft.DirectX.DirectInput;

namespace Gablarski.Input.DirectInput
{
	public class DirectInputProvider
		: IInputProvider
	{
		public event EventHandler<InputStateChangedEventArgs> InputStateChanged;

		public string DisplayName
		{
			get { return "DirectInput"; }
		}

		public void Attach (IntPtr window)
		{
			int index = 2;
			this.running = true;

			lock (this.syncRoot)
			{
				List<AutoResetEvent> resets = new List<AutoResetEvent>();
				foreach (DeviceInstance di in Manager.GetDevices (DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly))
				{
					AutoResetEvent reset = new AutoResetEvent (false);

					var d = new Device (di.InstanceGuid);
					d.Properties.BufferSize = 10;
					d.SetCooperativeLevel (window, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
					d.SetDataFormat (DeviceDataFormat.Joystick);
					d.SetEventNotification (reset);
					d.Acquire();

					resets.Add (reset);
					this.joysticks.Add (di.InstanceGuid, d);
					this.joystickIndexes.Add (index++, di.InstanceGuid);
				}

				this.waits = new AutoResetEvent[this.joysticks.Count + 2];
				this.waits[0] = new AutoResetEvent (false);
				this.keyboard = new Device (SystemGuid.Keyboard);
				this.keyboard.Properties.BufferSize = 10;
				this.keyboard.SetCooperativeLevel (window, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
				this.keyboard.SetDataFormat (DeviceDataFormat.Keyboard);
				this.keyboard.SetEventNotification (this.waits[0]);
				this.keyboard.Acquire();

				this.waits[1] = new AutoResetEvent (false);
				this.mouse = new Device (SystemGuid.Mouse);
				this.mouse.Properties.BufferSize = 10;
				this.mouse.SetCooperativeLevel (window, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
				this.mouse.SetDataFormat (DeviceDataFormat.Mouse);
				this.mouse.SetEventNotification (this.waits[1]);
				this.mouse.Acquire();

				resets.CopyTo (this.waits, 2);
			}

			(this.inputRunnerThread = new Thread (InputRunner)
			{
				Name = "DirectInput Runner",
				IsBackground = true
			}).Start();
		}

		public void SetBindings (IEnumerable<CommandBinding> bindings)
		{
			lock (this.syncRoot)
			{
				this.joystickBindings = new Dictionary<Guid, Dictionary<int, Command>>();
				foreach (CommandBinding binding in bindings)
				{
					string[] parts = binding.Input.Split ('|');
					Guid deviceGuid = new Guid (parts[0]);
					if (deviceGuid == SystemGuid.Keyboard || deviceGuid == SystemGuid.Mouse)
					{
						//TODO
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
			this.running = false;

			this.waits[0].Set(); // Keyboard wait always present, forces out of possible deadlock.
			lock (this.syncRoot)
			{
				for (int i = 0; i < this.waits.Length; ++i)
					this.waits[i].Set();

				this.inputRunnerThread.Join();
				this.inputRunnerThread = null;

				this.mouse.Unacquire();
				this.mouse.Dispose();

				this.keyboard.Unacquire();
				this.keyboard.Dispose();

				foreach (Device d in joysticks.Values)
				{
					d.Unacquire();
					d.Dispose();
				}

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

		public string GetNiceInputName (string input)
		{
			if (input == null || input.Trim() == String.Empty)
				throw new ArgumentNullException ("input");
			if (!input.Contains ("|"))
				throw new FormatException ("Input was in an unrecognized format.");

			string[] parts = input.Split ('|');
			Guid deviceGuid = new Guid (parts[0]);
			
			return GetNiceInputName (parts[1], new Device (deviceGuid));
		}

		public string EndRecord()
		{
			string garbage;
			return EndRecord (out garbage);
		}

		public string EndRecord (out string niceName)
		{
			if (!this.recording)
				throw new InvalidOperationException ("Not recording");

			this.recording = false;
			lock (this.syncRoot)
			{
				niceName = (this.lastRecording != null) ? GetNiceInputName (this.lastRecording) : null;
				return this.lastRecording;
			}
		}

		public void Dispose()
		{
			Detach();
		}

		private bool running;
		
		private bool recording;
		//private readonly Stack<string> recordings = new Stack<string>();
		private string lastRecording;

		private readonly Dictionary<Guid, Device> joysticks = new Dictionary<Guid, Device>();
		private readonly Dictionary<int, Guid> joystickIndexes = new Dictionary<int, Guid>();
		private Dictionary<Guid, Dictionary<int, Command>> joystickBindings;

		private readonly object syncRoot = new object();
		private MutableLookup<Guid, string> currentStates = new MutableLookup<Guid,string>();

		private AutoResetEvent[] waits;
		
		private Device keyboard;
		private Device mouse;

		private Thread inputRunnerThread;
		

		private string GetNiceInputName (string input, Device device)
		{
			switch (device.DeviceInformation.DeviceType)
			{
				case DeviceType.Keyboard:
					return input;

				case DeviceType.Mouse:
					return "Mouse " + input;

				case DeviceType.Joystick:
				case DeviceType.Gamepad:
					string[] parts = input.Split (';');

					string r = device.Properties.ProductName + " " +
					           device.GetObjectInformation (Int32.Parse (parts[0]), ParameterHow.ByOffset).Name;
					if (parts.Length == 2)
						r += parts[1];

					return r;

				default:
					return "Unknown";
			}
		}

		private void OnInputStateChanged (InputStateChangedEventArgs e)
		{
			EventHandler<InputStateChangedEventArgs> handler = InputStateChanged;
			if (handler != null)
				handler (this, e);
		}

		private void InputRunner()
		{
			Dictionary<Device, Dictionary<int, int>> objectInitial = new Dictionary<Device, Dictionary<int, int>>();
			Dictionary<Device, Dictionary<int, InputRange>> objectRanges = new Dictionary<Device, Dictionary<int, InputRange>>();
			Dictionary<Device, HashSet<int>> buttons = new Dictionary<Device, HashSet<int>>();

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

			while (this.running)
			{
				int waited = WaitHandle.WaitAny (this.waits);
				lock (this.syncRoot)
				{
					switch (waited)
					{
						case 0: // Keyboard
							break;

						case 1: // Mouse
							break;

						default:
							var d = this.joysticks[this.joystickIndexes[waited]];
							var currentButtons = buttons[d];
							var currentInitials = objectInitial[d];
							var currentRanges = objectRanges[d];

							BufferedDataCollection buffer = d.GetBufferedData();
							if (buffer == null)
								continue;

							for (int i = 0; i < buffer.Count; i++)
							{
								BufferedData bd = buffer[i];

								if (this.recording)
								{
									int initial;
									if (!currentInitials.TryGetValue (bd.Offset, out initial))
									{
										if (!currentButtons.Contains (bd.Offset))
										{
											currentInitials[bd.Offset] = bd.Data;
											currentRanges.Add (bd.Offset, d.Properties.GetRange (ParameterHow.ByOffset, bd.Offset));
										}
										else
											this.lastRecording = String.Format ("{0}|{1}", d.DeviceInformation.InstanceGuid, bd.Offset);
									}
									else
									{
										InputRange range = currentRanges[bd.Offset];
										int delta = Math.Abs (initial - bd.Data);
										if (((float)delta / (range.Max - range.Min)) > 0.25) // >25% change
											this.lastRecording = String.Format ("{0}|{1};{2}", d.DeviceInformation.InstanceGuid, bd.Offset, ((initial > bd.Data) ? "+" : "-"));
									}
								}
								else
								{
									if (this.joystickBindings == null)
										continue;

									Dictionary<int, Command> binds;
									if (!this.joystickBindings.TryGetValue (d.DeviceInformation.InstanceGuid, out binds) || binds.Count == 0)
										continue;
									 
									Command c;
									if (!binds.TryGetValue (bd.Offset, out c))
										continue;

									if (currentButtons.Contains (bd.Offset))
										OnInputStateChanged (new InputStateChangedEventArgs ((bd.Data == 128) ? InputState.On : InputState.Off));
									else
									{
										InputRange range = currentRanges[bd.Offset];

										double value = (bd.Data != -1) ? bd.Data : range.Max;
										OnInputStateChanged (new InputStateChangedEventArgs (InputState.Axis, (value / (range.Max - range.Min)) * 100));
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