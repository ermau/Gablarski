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

		public string ServerLogo
		{
			get;
			set;
		}

		internal void Serialize (IValueWriter writer)
		{
			writer.WriteString (this.ChannelIdentifyingType.AssemblyQualifiedName);
			writer.WriteString (this.UserIdentifyingType.AssemblyQualifiedName);
			writer.WriteString (this.ServerName);
			writer.WriteString (this.ServerDescription);
			writer.WriteString (this.ServerLogo);
		}

		internal void Deserialize (IValueReader reader)
		{
			this.ChannelIdentifyingType = Type.GetType (reader.ReadString());
			this.UserIdentifyingType = Type.GetType (reader.ReadString());
			this.ServerName = reader.ReadString();
			this.ServerDescription = reader.ReadString();
			this.ServerLogo = reader.ReadString();
		}
	}
}