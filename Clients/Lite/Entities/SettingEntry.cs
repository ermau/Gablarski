using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Clients.Windows.Entities
{
	public class SettingEntry
	{
		public virtual int ID
		{
			get; private set;
		}

		public virtual string Name
		{
			get; set;
		}

		public virtual string Value
		{
			get; set;
		}
	}
}