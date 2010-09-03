using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Clients.Input;

namespace Gablarski.Clients.Windows.Entities
{
	public class CommandBindingEntry
	{
		public CommandBindingEntry()
		{
		}

		public CommandBindingEntry (CommandBinding binding)
		{
			if (binding == null)
				throw new ArgumentNullException ("binding");

			ProviderType = binding.Provider.GetType().Name;
			Command = binding.Command;
			Input = binding.Input;
		}

		public string ProviderType
		{
			get;
			set;
		}

		public Command Command
		{
			get;
			set;
		}

		public string Input
		{
			get;
			set;
		}
	}
}