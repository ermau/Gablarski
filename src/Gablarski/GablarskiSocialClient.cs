using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gablarski.Client;
using Tempest;
using Tempest.Providers.Network;
using Tempest.Social;

namespace Gablarski
{
	public class GablarskiSocialClient
		: SocialClient
	{
		public GablarskiSocialClient (Person person, RSAAsymmetricKey key)
			: base (new NetworkClientConnection (SocialProtocol.Instance, key), person)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			this.key = key;
		}

		public async Task<Group> StartGroupWithAsync (Person person)
		{
			if (person == null)
				throw new ArgumentNullException ("person");

			Group group = await CreateGroupAsync().ConfigureAwait (false);
			if (group == null)
				return null;

			Invitation invitation = await InviteToGroupAsync (group, person).ConfigureAwait (false);
			if (invitation == null)
				return null;

			return group;
		}

		public async Task ConnectToGroupVoice (Group group)
		{
			if (group == null)
				throw new ArgumentNullException ("group");

			
		}

		private readonly RSAAsymmetricKey key;
	}
}