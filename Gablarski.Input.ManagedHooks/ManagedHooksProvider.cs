using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gablarski.Clients.Input;
using Kennedy.ManagedHooks;

namespace Gablarski.Input.ManagedHooks
{
	public class ManagedHooksProvider
		: IInputProvider
	{
		#region Implementation of IInputProvider

		public event EventHandler<InputStateChangedEventArgs> InputStateChanged;

		public string DisplayName
		{
			get { return "System Hooks"; }
		}

		/// <summary>
		/// Attaches the input provider to listen for the given settings.
		/// </summary>
		/// <param name="settings">The settings provided by <see cref="IInputProvider.EndRecord()"/>.</param>
		public void Attach (IntPtr window, string settings)
		{
			if (!String.IsNullOrEmpty (settings))
				Parse (settings);

			khook = new KeyboardHook();
			khook.InstallHook();
			khook.KeyboardEvent += OnKeyboardEvent;
		}

		public void Detach()
		{
			khook.KeyboardEvent -= OnKeyboardEvent;
			khook.UninstallHook();
			khook.Dispose();
			khook = null;
		}

		/// <summary>
		/// Starts recording input combinations for saving.
		/// </summary>
		public void BeginRecord()
		{
			if (khook == null || !khook.IsHooked)
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
			int ival;
			if (!Int32.TryParse (inputSettings.Substring (1), out ival))
				return ((Keys)ival).ToString();

			return String.Empty;
		}

		/// <summary>
		/// Stops recording input combinations and returns the latest combination.
		/// </summary>
		/// <returns>The latest input combination, <c>null</c> if no input was gathered.</returns>
		public string EndRecord()
		{
			this.recording = false;

			//return (keyboard) ? "k" + this.keys : "m" + MouseButtons;
			return (this.keys != Keys.None) ? "k" + (int)this.keys : null;
		}

		public string EndRecord (out string niceName)
		{
			niceName = (this.keys != Keys.None) ? this.keys.ToString() : null;
			return EndRecord();
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing)
			{
				if (this.khook != null)
					this.khook.Dispose();
			}
		}

		~ManagedHooksProvider()
		{
			Dispose (false);
		}

		#endregion

		private Keys keys = Keys.None;
		private KeyboardHook khook;
		private bool recording;
		private InputState inputState;

		private void OnKeyboardEvent (KeyboardEvents kEvent, Keys key)
		{
			if (this.recording)
				this.keys = key;
			
			if (this.keys == key || this.recording)
			{
				if (this.inputState != InputState.On && kEvent == KeyboardEvents.KeyDown)
					this.inputState = InputState.On;
				else if (kEvent == KeyboardEvents.KeyUp)
					this.inputState = InputState.Off;
				else
					return;

				var ev = this.InputStateChanged;
				if (ev != null)
					ev (this, new InputStateChangedEventArgs (this.inputState));
			}
		}

		public void Parse (string value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (value.Trim() == String.Empty)
				throw new ArgumentException ("Empty value.", "value");

			//if (value[0] == 'm')
			//{
			//    int ival;
			//    if (!Int32.TryParse (value.Substring (1), out ival))
			//        throw new FormatException();
			//    else
			//        return new PushToTalk ((MouseButtons)ival);
			//}
			if (value[0] == 'k')
			{
			    int ival;
			    if (!Int32.TryParse (value.Substring (1), out ival))
			        throw new FormatException();
			    else
			    	this.keys = (Keys)ival;
			}
			else
				throw new FormatException();
		}
	}
}