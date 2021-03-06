﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Clients.Persistence
{
	public class ServerEntry
	{
		public ServerEntry (int id)
		{
			this.Id = id;
		}

		public virtual int Id
		{
			get;
			private set;
		}

		public virtual string Name
		{
			get;
			set;
		}

		public virtual string Host
		{
			get;
			set;
		}

		public virtual int Port
		{
			get;
			set;
		}

		public virtual string ServerPassword
		{
			get;
			set;
		}

		public virtual string UserNickname
		{
			get;
			set;
		}

		public virtual string UserPhonetic
		{
			get;
			set;
		}

		public virtual string UserName
		{
			get;
			set;
		}

		public virtual string UserPassword
		{
			get;
			set;
		}
	}
}