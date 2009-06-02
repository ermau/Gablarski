using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Gablarski.Server
{
	public class ServerSettings
		: INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private string name = "Gablarski Server";
		public virtual string Name
		{
			get { return this.name; }
			set
			{
				if (value != this.name)
				{
					this.name = value;
					OnPropertyChanged ("Name");
				}
			}
		}

		private string description = "Default Gablarski Server";
		public virtual string Description
		{
			get { return this.description; }
			set
			{
				if (value != this.description)
				{
					this.description = value;
					OnPropertyChanged ("Description");
				}
			}
		}

		protected void OnPropertyChanged (string propertyName)
		{
			var changed = this.PropertyChanged;
			if (changed != null)
				changed (this, new PropertyChangedEventArgs (propertyName));
		}
	}
}