using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Cadenza;
using Gablarski.Clients.Windows.Entities;
using CommandBinding = Gablarski.Clients.Input.CommandBinding;
using Gablarski.Clients.Input;

namespace Gablarski.Clients.Windows
{
	public class CommandBindingSettingEntry
		: CommandBindingEntry, INotifyPropertyChanged
	{
		public CommandBindingSettingEntry (IInputProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");

			this.provider = provider;
		}

		public CommandBindingSettingEntry (IInputProvider provider, CommandBindingEntry entry)
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
				if (Recording)
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
			get { return new KeyValuePair<string, Command>(BindingListViewModel.SpaceEnum (Command.ToString()), Command); }
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
			var changed = PropertyChanged;
			if (changed != null)
				changed (this, e);
		}
	}

	public class BindingListViewModel
		: INotifyPropertyChanged
	{
		private readonly IntPtr window;

		public BindingListViewModel (IntPtr window, ICommand recordCommand)
		{
			if (recordCommand == null)
				throw new ArgumentNullException ("recordCommand");

			this.RecordCommand = recordCommand;
			this.window = window;
			this.RemoveCommand = new AnonymousCommand (o =>
			{
				var binding = (o as CommandBindingSettingEntry);
				if (binding == null)
					return;

				this.bindings.Remove (binding);
			},

			o => o is CommandBindingSettingEntry);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private IInputProvider inputProvider;
		public IInputProvider InputProvider
		{
			get { return this.inputProvider; }
			set
			{
				if (this.inputProvider == value)
					return;

				if (this.inputProvider != null)
				{
					this.inputProvider.Detach();
					this.inputProvider.Dispose();
				}

				this.inputProvider = value;
				if (this.inputProvider == null)
					return;

				this.inputProvider.Attach (this.window);
				OnPropertyChanged (new PropertyChangedEventArgs ("InputProvider"));

				this.bindings = new ObservableCollection<CommandBindingSettingEntry> (Persistance.GetCommandBindings()
					.Where (b => value.GetType().Name == b.ProviderType)
					.Select (b => new CommandBindingSettingEntry (value, b)));

				OnPropertyChanged (new PropertyChangedEventArgs ("Bindings"));
			}
		}

		public IDictionary<string, Command> Commands
		{
			get { return ((Command[])Enum.GetValues (typeof (Command))).Skip (1).ToDictionary (c => SpaceEnum (c.ToString()), c => c); }
		}

		public ICollection<CommandBindingSettingEntry> Bindings
		{
			get { return this.bindings; }
		}

		public ICommand RecordCommand
		{
			get;
			private set;
		}

		public ICommand RemoveCommand
		{
			get;
			private set;
		}

		private ObservableCollection<CommandBindingSettingEntry> bindings;

		private void OnPropertyChanged (PropertyChangedEventArgs e)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler (this, e);
		}

		internal static string SpaceEnum (string enumName)
		{
			string name = enumName[0].ToString();
			for (int i = 1; i < enumName.Length; ++i)
			{
				if (Char.IsUpper (enumName[i]))
					name += " ";

				name += enumName[i];
			}

			return name;
		}
	}
}