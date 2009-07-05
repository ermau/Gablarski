using System;
using Gablarski.Server;

namespace Gablarski
{
	public class ServerInfo
	{
		internal ServerInfo (IValueReader reader)
		{
			this.Deserialize (reader);
		}

		internal ServerInfo (ServerSettings settings, Type channelIdentifingType, Type userIdentifyingType)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");
			if (channelIdentifingType == null)
				throw new ArgumentNullException("channelIdentifingType");
			if (userIdentifyingType == null)
				throw new ArgumentNullException("userIdentifyingType");

			this.ServerName = settings.Name;
			this.ServerDescription = settings.Description;

			this.ChannelIdentifyingType = channelIdentifingType;
			this.UserIdentifyingType = userIdentifyingType;
		}

		/// <summary>
		/// Gets the name of the server.
		/// </summary>
		public string ServerName
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the server description.
		/// </summary>
		public string ServerDescription
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the type identifying channels.
		/// </summary>
		public Type ChannelIdentifyingType
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the type identifying users.
		/// </summary>
		public Type UserIdentifyingType
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the url of the server's logo.
		/// </summary>
		public string ServerLogo
		{
			get;
			private set;
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