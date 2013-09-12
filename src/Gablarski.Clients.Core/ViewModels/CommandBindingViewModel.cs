using System;
using System.Collections.Generic;
using System.ComponentModel;
using Cadenza;
using Gablarski.Clients.Input;
using Gablarski.Clients.Persistence;

namespace Gablarski.Clients.ViewModels
{
	public class CommandBindingViewModel
		: CommandBindingEntry, INotifyPropertyChanged
	{
		public CommandBindingViewModel (IInputProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");

			this.provider = provider;
		}

		public CommandBindingViewModel (IInputProvider provider, CommandBindingEntry entry)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");

			this.provider = provider;
			Input = entry.Input;
			ProviderType = entry.ProviderType;
			Command = entry.Command;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public string NiceInput
		{
			get
			{
				if (Input.IsNullOrWhitespace())
					return "Set binding";
				if (this.Recording)
					return "Do something already..";
				if (this.provider != null)
					return this.provider.GetNiceInputName (Command, Input);

				return "Error";
			}
		}

		public bool Recording
		{
			get { return this.recording; }
			set
			{
				if (value == this.Recording)
					return;

				this.recording = value;
				OnPropertyChanged (new PropertyChangedEventArgs ("Recording"));
				OnPropertyChanged (new PropertyChangedEventArgs ("NiceInput"));
			}
		}

		public KeyValuePair<string, Command> BoundCommand
		{
			get { return new KeyValuePair<string, Command> (BindingListViewModel.SpaceEnum (Command.ToString()), Command); }
			set
			{
				if (value.Value == Command)
					return;

				Command = value.Value;
				OnPropertyChanged (new PropertyChangedEventArgs ("Command"));
				OnPropertyChanged (new PropertyChangedEventArgs ("BoundCommand"));
			}
		}

		private readonly IInputProvider provider;
		private bool recording;

		private void OnPropertyChanged (PropertyChangedEventArgs e)
		{
			var changed = this.PropertyChanged;
			if (changed != null)
				changed (this, e);
		}
	}
}