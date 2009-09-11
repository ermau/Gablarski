using Gablarski.Audio;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public class ClientAudioSource : AudioSource
	{
		internal ClientAudioSource (AudioSource source, IClientConnection client)
			: base (source.Name, source.Id, source.OwnerId, source.Channels, source.Bitrate, source.Frequency, source.FrameSize, source.Complexity, source.IsMuted)
		{
			this.client = client;
		}

		public bool IsIgnored
		{
			get; private set;
		}

		public bool ToggleIgnore ()
		{
			return (this.IsIgnored = !this.IsIgnored);
		}

		public void ToggleMute ()
		{
			this.client.Send (new RequestMuteMessage { Target = this.Id, Type = MuteType.AudioSource, Unmute = !this.IsMuted });
		}

		protected IClientConnection client;
	}
}