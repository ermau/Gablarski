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
				this.keys = ParseKeySettings (settings);

			this.keyboardWait = new AutoResetEvent (false);
			this.keyboard = new Device (SystemGuid.Keyboard);
			this.keyboard.SetCooperativeLevel (window, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
			this.keyboard.SetDataFormat (DeviceDataFormat.Keyboard);
			this.keyboard.SetEventNotification (this.keyboardWait);
			this.keyboard.Acquire();

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

				this.pollThread.Join();
				this.pollThread = null;
			}

			if (this.keyboard == null)
				return;

			this.keyboard.Unacquire();
			this.keyboard.SetEventNotification (null);
			this.keyboard.Dispose();
			this.keyboard = null;

			this.keyboardWait.Close();
			this.keyboardWait = null;
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

		private Device mouse;

		private Thread pollThread;

		private string GetSettings ()
		{
			if (this.keyboardState == null)
				return null;

			StringBuilder builder = new StringBuilder();
			foreach (var kvp in EnumerateKeyboardState (this.keyboardState).Where (kvp => kvp.Value))
			{
				builder.Append ("k");
				builder.Append ((int)kvp.Key);
				builder.Append (";");
			}

			return builder.ToString();
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

		private bool CheckInputState (bool any)
		{
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

			while (this.running)
			{
				keyboardWait.WaitOne();

				this.keyboard.Poll();

				try
				{
					this.keyboardState = this.keyboard.GetCurrentKeyboardState();

					bool state = CheckInputState (this.recording);
					bool changed = (state != previous);
					previous = state;

					if (changed)
						OnStateChanged (new InputStateChangedEventArgs ((state) ? InputState.On : InputState.Off));
				}
				catch (NotAcquiredException)
				{
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