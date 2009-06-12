using System;
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

		public Type ChannelIdentifyingType
		{
			get;
			set;
		}

		public Type UserIdentifyingType
		{
			get;
			set;
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteString (this.ServerName);
			writer.WriteString (this.ServerDescription);
			writer.WriteString (this.ChannelIdentifyingType.AssemblyQualifiedName);
			writer.WriteString (this.UserIdentifyingType.AssemblyQualifiedName);
		}

		internal void Deserialize (IValueReader reader)
		{
			this.ServerName = reader.ReadString();
			this.ServerDescription = reader.ReadString();
			this.ChannelIdentifyingType = Type.GetType (reader.ReadString());
			this.UserIdentifyingType = Type.GetType (reader.ReadString());
		}
	}
}