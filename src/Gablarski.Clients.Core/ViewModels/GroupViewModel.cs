using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gablarski.Client;
using GalaSoft.MvvmLight;
using Tempest.Social;

namespace Gablarski.Clients.ViewModels
{
	class GroupViewModel
		: ViewModelBase
	{
		private readonly GablarskiClient client;

		public GroupViewModel (GablarskiClient client, Group group)
		{
			Group = group;
			this.client = client;
		}

		public Group Group
		{
			get;
			private set;
		}

		/*public IEnumerable<Person> Participants
		{
			get { return Group.Participants.Select(id => client.Social.WatchList.)}
		}*/
	}
}