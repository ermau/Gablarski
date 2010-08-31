using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Gablarski.Clients.Windows.Entities;
using CommandBinding = Gablarski.Clients.Input.CommandBinding;
using Gablarski.Clients.Input;

namespace Gablarski.Clients.Windows
{
	public class CommandBindingSettingEntry
		: CommandBindingEntry
	{
		public CommandBindingSettingEntry (int id)
			: base (id)
		{
		}

		public bool Recording
		{
			get;
			set;
		}
	}

	public class BindingListViewModel
		: INotifyPropertyChanged
	{
		private readonly IntPtr window;

		public BindingListViewModel (IntPtr window)
		{
			this.window = window;
			this.removeCommand = new AnonymousCommand (o =>
			{
				var binding = (o as CommandBindingEntry);
				if (binding == null)
					return;

				this.bindings.Add (binding);
			},

			o => o is CommandBinding);
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
				this.inputProvider.Attach (this.window);
				OnPropertyChanged (new PropertyChangedEventArgs ("InputProvider"));

				this.bindings = new ObservableCollection<CommandBindingEntry>(Persistance.GetCommandBindings().Where (b => value.GetType().Name == b.ProviderType));

				OnPropertyChanged (new PropertyChangedEventArgs ("Bindings"));
			}
		}

		public IEnumerable<CommandBindingEntry> Bindings
		{
			get { return this.bindings; }
		}

		private readonly ICommand removeCommand;
		private ObservableCollection<CommandBindingEntry> bindings;

		public ICommand RemoveCommand
		{
			get { return this.removeCommand; }
		}

		private void OnPropertyChanged (PropertyChangedEventArgs e)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler (this, e);
		}
	}
}