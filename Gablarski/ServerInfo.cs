using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;

namespace Gablarski
{
	public class ServerInfo
	{
		internal ServerInfo (ServerSettings settings)
		{
			this.ServerName = settings.Name;
			this.ServerDescription = settings.Description;
		}

		internal ServerInfo (IValueReader reader)
		{
			this.Deserialize (reader);
		}

		public string ServerName
		{
			get;
			set;
		}

		public string ServerDescription
		{
			get;
			set;
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteString (this.ServerName);
			writer.WriteString (this.ServerDescription);
		}

		internal void Deserialize (IValueReader reader)
		{
			this.ServerName = reader.ReadString();
			this.ServerDescription = reader.ReadString();
		}
	}
}