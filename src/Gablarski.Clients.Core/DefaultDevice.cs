using Gablarski.Audio;
using Gablarski.Clients.Properties;

namespace Gablarski.Clients
{
	public class DefaultDevice
		: IAudioDevice
	{
		public string Name
		{
			get { return Resources.Default; }
		}

		public void Dispose()
		{
		}
	}
}
