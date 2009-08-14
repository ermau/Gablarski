using System;
using Gablarski;
using Gablarski.Client;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace Gablarski.Clients.iPhone
{
	public class ServerTableDataSource
		: UITableViewDataSource
	{
		public ServerTableDataSource (ClientChannelManager channels, ClientUserManager users)
		{
			this.channelManager = channels;
			this.userManager = users;
			
			this.channelManager.CollectionChanged += (s, e) =>
			{
				this.channels = new List<ChannelInfo> (this.channelManager);
			};
			
			this.userManager.CollectionChanged += (s, e) =>
			{
				this.users = new List<ClientUser> (this.userManager);
				this.channelsToUsers = this.users.ToDictionary (u => u.CurrentChannelId);
			};
		}
		
		public override int NumberOfSections (MonoTouch.UIKit.UITableView tableView)
		{
			return channels.Count;
		}
		
		public override int RowsInSection (MonoTouch.UIKit.UITableView tableview, int section)
		{
			return this.channelsTousers[this.channels[section].ChannelId].Count();
		}
		
		public override bool CanMoveRow (MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			//return base.CanMoveRow (tableView, indexPath);
			return true;
		}

		public override UITableViewCell GetCell (MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			throw new System.NotImplementedException ();
		}
		
		private Dictionary<int, ClientUser> channelsToUsers;
		private List<ChannelInfo> channels;
		private List<ClientUser> users;

		private readonly ClientChannelManager channelManager;
		private readonly ClientUserManager userManager;
	}
}