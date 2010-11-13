using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Gablarski.Clients.Windows.Entities
{
	[DebuggerDisplay ("{Name} = {Value}")]
	public class SettingEntry
	{
		public SettingEntry (int settingId)
		{
			this.Id = settingId;
		}

		public virtual int Id
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