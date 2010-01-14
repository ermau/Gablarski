using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Gablarski.LocalServer.Mappings
{
	public class ChannelMap
		: ClassMap<ChannelInfo>
	{
		public ChannelMap()
		{
			Id (x => x.ChannelId, "channelID");
			Map (x => x.ParentChannelId, "channelParentID");
			Map (x => x.Name, "channelName");
			Map (x => x.Description, "channelDescription");
			Map (x => x.UserLimit, "channelLimit");
		}
	}
}