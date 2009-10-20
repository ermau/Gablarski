using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Gablarski.Clients.Input;
using Microsoft.DirectX.DirectInput;

namespace Gablarski.Input.DirectInput
{
	public class DirectInputProvider
		: IInputProvider
	{
		#region Implementation of IInputProvider

		public event EventHandler<InputStateChangedEventArgs> InputStateChanged;

		/// <summary>
		/// The displayable name of the input provider.
		/// </summary>
		public string DisplayName
		{
			get { return "DirectInput"; }
		}

		/// <summary>
		/// Attaches the input provider to listen for the given settings.
		/// </summary>
		/// <param name="settings">The settings provided by <see cref="IInputProvider.EndRecord()"/>. <c>null</c> or <c>String.Empty</c> if not yet set.</param>
		public void Attach (IntPtr window, string settings)
		{
			if (window == IntPtr.Zero)
				throw new ArgumentException ("Invalid window", "window");

			if (!String.IsNullOrEmpty (settings))
			{
				if (settings.Contains ("m"))
					this.mouseButton = ParseMouseSettings (settings);
				else
					this.keys = ParseKeySettings (settings);
			}

			this.keyboardWait = new AutoResetEvent (false);
			this.keyboard = new Device (SystemGuid.Keyboard);
			this.keyboard.SetCooperativeLevel (window, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
			this.keyboard.SetDataFormat (DeviceDataFormat.Keyboard);
			this.keyboard.SetEventNotification (this.keyboardWait);
			this.keyboard.Acquire();

			this.mouseWait = new AutoResetEvent (false);
			this.mouse = new Device (SystemGuid.Mouse);
			this.mouse.SetCooperativeLevel (window, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
			this.mouse.SetDataFormat (DeviceDataFormat.Mouse);
			this.mouse.SetEventNotification (this.mouseWait);
			this.mouse.Acquire ();

			this.running = true;

			this.pollThread = new Thread (Poller);
			this.pollThread.IsBackground = true;
			this.pollThread.Name = "DirectInput Poller";
			this.pollThread.Start();
		}

		/// <summary>
		/// Shuts down and detatches the input provider.
		/// </summary>
		public void Detach()
		{
			this.running = false;

			if (this.pollThread != null)
			{
				this.keyboardWait.Set();
				this.mouseWait.Set();

				this.pollThread.Join();
				this.pollThread = null;
			}

			ShutdownDevice (ref this.keyboard, ref this.keyboardWait);
			ShutdownDevice (ref this.mouse, ref this.mouseWait);
		}

		/// <summary>
		/// Starts recording input combinations for saving.
		/// </summary>
		public void BeginRecord()
		{
			if (!this.running)
				throw new InvalidOperationException ("Must be attached before recording.");

			this.recording = true;
		}

		/// <summary>
		/// Gets a nice display name for the given <paramref name="inputSettings"/>.
		/// </summary>
		/// <param name="inputSettings">The input settings to beautify.</param>
		/// <returns>The nice display name for <paramref name="inputSettings"/>.</returns>
		public string GetNiceInputName (string inputSettings)
		{
			if (String.IsNullOrEmpty (inputSettings))
				return String.Empty;

			if (inputSettings.Contains ("m"))
			{
				int mouseb;
				if (Int32.TryParse (inputSettings.Substring (1).Substring (0, inputSettings.Length - 2), out mouseb))
					return "Mouse " + (mouseb + 1);
				else
					return String.Empty;
			}
			else
				return GetNiceString (ParseKeySettings (inputSettings));
		}

		/// <summary>
		/// Stops recording input combinations and returns the latest combination.
		/// </summary>
		/// <returns>The latest input combination, <c>null</c> if no input was gathered.</returns>
		public string EndRecord()
		{
			this.recording = false;

			return GetSettings();
		}

		/// <summary>
		/// Stops recording input combinations and returns the latest combination.
		/// </summary>
		/// <param name="niceName">A human-readable string for for the input combination.</param>
		/// <returns>The latest input combination, <c>null</c> if not input was gathered.</returns>
		public string EndRecord (out string niceName)
		{
			this.recording = false;

			string settings = GetSettings();
			niceName = GetNiceInputName (settings);

			return settings;
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			Detach();
		}

		#endregion

		private volatile bool running;
		private bool recording;

		private Key[] keys;
		private KeyboardState keyboardState;
		private AutoResetEvent keyboardWait;
		private Device keyboard;

		private int mouseButton = -1;
		private MouseState mouseState;
		private AutoResetEvent mouseWait;
		private Device mouse;

		private Thread pollThread;

		private void ShutdownDevice (ref Device d, ref AutoResetEvent wait)
		{
			if (d != null)
			{
				d.Unacquire ();
				d.SetEventNotification (null);
				d.Dispose ();
				d = null;
			}

			if (wait != null)
			{
				wait.Close ();
				wait = null;
			}
		}

		private string GetSettings ()
		{
			string settings = null;

			if (this.keyboardState != null)
			{
				StringBuilder builder = new StringBuilder ();
				foreach (var kvp in EnumerateKeyboardState (this.keyboardState).Where (kvp => kvp.Value))
				{
					builder.Append ("k");
					builder.Append ((int)kvp.Key);
					builder.Append (";");
				}

				settings = builder.ToString ();
			}
			else
			{
				byte[] buttons = this.mouseState.GetMouseButtons();

				int button = -1;
				for (int i = 0; i < buttons.Length; ++i)
				{
					if (buttons[i] != 0)
					{
						button = i;
						break;
					}
				}

				if (button == -1)
					return null;

				settings = "m" + button + ";";
			}

			this.keyboardState = null;
			return settings;
		}

		private static int ParseMouseSettings (string settings)
		{
			string[] pieces = settings.Split (';');

			int button;
			if (pieces.Length > 0 && Int32.TryParse (pieces[0].Substring (1), out button))
				return button;
			else
				return -1;
		}

		private static Key[] ParseKeySettings (string settings)
		{
			string[] pieces = settings.Split (';');

			var ksettings = pieces.Where (s => s.Length > 0 && s[0] == 'k').ToList();
			Key[] keys = new Key[ksettings.Count];
			for (int i = 0; i < ksettings.Count; ++i)
			{
				int kval;
				if (Int32.TryParse (ksettings[i].Substring (1), out kval))
					keys[i] = (Key)kval;
			}

			return keys;
		}

		private static IEnumerable<KeyValuePair<Key, bool>> EnumerateKeyboardState (KeyboardState s)
		{
			string[] names = Enum.GetNames (typeof (Key));
			for (int i = 0; i < names.Length; ++i)
			{
				string name = names[i];

				Key k = (Key)Enum.Parse (typeof (Key), name);

				yield return new KeyValuePair<Key, bool> (k, s[k]);
			}
		}

		private bool CheckKeyboardState (bool any)
		{
			if ((this.keys == null || this.keys.Length == 0) && !any)
			{
				this.keyboardState = null;
				return false;
			}

			KeyboardState s = this.keyboardState;

			if (!any)
			{
				Key[] k = this.keys;
				if (k == null)
					return false;

				if (k.Length == 0)
					return false;

				for (int i = 0; i < k.Length; ++i)
				{
					if (!s[k[i]])
						return false;
				}

				return true;
			}
			else
			{
				return EnumerateKeyboardState (s).Any (kvp => kvp.Value);
			}
		}

		private bool CheckMouseState (bool any)
		{
			if (this.mouseButton == -1 && !any)
				return false;

			byte[] buttons = this.mouseState.GetMouseButtons ();
			if (!any)
				return (buttons[this.mouseButton] != 0);
			else
				return buttons.Any (i => i != 0);
		}

		private static string GetNiceString (Key[] ks)
		{
			StringBuilder nice = new StringBuilder();

			for (int i = 0; i < ks.Length; ++i)
			{
				if (nice.Length != 0)
					nice.Append (" + ");

				nice.Append (ks[i].ToString());
			}

			return nice.ToString();
		}

		private static string SpaceCamel (string s)
		{
			StringBuilder builder = new StringBuilder (s.Length * 2);
			for (int i = 0; i < s.Length; ++i)
			{
				if (Char.IsUpper (s[i]))
					builder.Append (" ");

				builder.Append (s[i]);
			}

			return builder.ToString();
		}

		private void Poller()
		{
			bool previous = false;

			WaitHandle[] waits = new[] { keyboardWait, mouseWait };

			while (this.running)
			{
				switch (AutoResetEvent.WaitAny (waits))
				{
					case 0:
					{
						this.keyboard.Poll ();

						try
						{
							this.keyboardState = this.keyboard.GetCurrentKeyboardState ();

							bool state = CheckKeyboardState (this.recording);
							bool changed = (state != previous);
							previous = state;

							if (changed)
								OnStateChanged (new InputStateChangedEventArgs ((state) ? InputState.On : InputState.Off));
						}
						catch (NotAcquiredException)
						{
						}

						break;
					}

					case 1:
					{
						this.mouse.Poll ();

						try
						{
							this.mouseState = this.mouse.CurrentMouseState;

							bool state = CheckMouseState (this.recording);
							bool changed = (state != previous);
							previous = state;

							if (changed)
								OnStateChanged (new InputStateChangedEventArgs ((state) ? InputState.On : InputState.Off));
						}
						catch (NotAcquiredException)
						{
						}

						break;
					}
				}
			}
		}

		private void OnStateChanged (InputStateChangedEventArgs e)
		{
			var changed = this.InputStateChanged;
			if (changed != null)
				changed (this, e);
		}
	}
}