using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Clients.Input
{
	/// <summary>
	/// Represents the action that a keybinding is meant to execute.
	/// </summary>
	public enum Command
	{
		/// <summary>
		/// Push to talk.
		/// </summary>
		Talk = 0,
	}

	/// <summary>
	/// Represents a keybinding
	/// </summary>
	public class CommandBinding
	{
		/// <summary>
		/// Creates a new keybinding.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="provider"/> or <paramref name="input"/> is <c>null</c>.</exception>
		public CommandBinding (IInputProvider provider, Command action, string input)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");
			if (input == null)
				throw new ArgumentNullException ("input");

			Provider = provider;
			Command = action;
			Input = input;
		}

		/// <summary>
		/// Gets the provider the keybind is registered for.
		/// </summary>
		public IInputProvider Provider
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the action the keybind is to execute.
		/// </summary>
		public Command Command
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the <see cref="Provider"/>'s recognized input.
		/// </summary>
		public string Input
		{
			get;
			private set;
		}
	}
}