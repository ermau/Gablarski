using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Clients.Input;

namespace Gablarski.Clients.Windows.Entities
{
	public class CommandBindingEntry
	{
		public CommandBindingEntry (int id)
		{
			Id = id;
		}

		public virtual int Id
		{
			get;
			private set;
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