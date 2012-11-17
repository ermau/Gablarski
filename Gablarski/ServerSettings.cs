using Tempest;

namespace Gablarski
{
	public class ServerSettings
	{
		public string Name
		{
			get;
			set;
		}

		public string Password
		{
			get;
			set;
		}
	}

	public sealed class ServerSettingsSerializer
		: ISerializer<ServerSettings>
	{
		public static readonly ServerInfoSerializer Instance = new ServerInfoSerializer();

		public void Serialize (ISerializationContext context, IValueWriter writer, ServerSettings element)
		{
			writer.WriteString (element.Name);
		}

		public ServerSettings Deserialize (ISerializationContext context, IValueReader reader)
		{
			var settings = new ServerSettings();
			settings.Name = reader.ReadString();

			return settings;
		}
	}
}