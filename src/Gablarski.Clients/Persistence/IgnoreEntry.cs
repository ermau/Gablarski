using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Clients.Windows.Entities
{
	public class IgnoreEntry
	{
		public IgnoreEntry (int id)
		{
			Id = id;
		}

		public virtual int Id
		{
			get;
			private set;
		}

		public virtual int ServerId
		{
			get;
			set;
		}

		public virtual string Username
		{
			get;
			set;
		}
	}
}