using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gablarski.Clients.Windows.Properties;

namespace Gablarski.Clients.Windows
{
	public class PlayerTreeView
		: TreeView
	{
		public PlayerTreeView()
		{
			this.ImageList = new ImageList
			{
				TransparentColor = Color.Transparent,
				ImageSize = new Size (16, 16),
				ColorDepth = ColorDepth.Depth24Bit
			};

			this.ImageList.Images.Add ("server",	Resources.ServerImage);
			this.ImageList.Images.Add ("channel",	Resources.UsersImage);
			this.ImageList.Images.Add ("silent",	Resources.SoundNoneImage);
			this.ImageList.Images.Add ("talking",	Resources.SoundImage);
			this.ImageList.Images.Add ("music",		Resources.MusicImage);
		}

		public void SetServerNode (TreeNode node)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<TreeNode>)SetServerNode, node);
				return;
			}

			this.BeginUpdate();

			this.Nodes.Clear();
			this.serverNode = node;
			this.serverNode.ImageKey = "server";
			this.serverNode.SelectedImageKey = "server";
			this.Nodes.Add (node);

			this.EndUpdate();
		}

		public void AddChannel (Channel channel, IdentifyingTypes idTypes)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<Channel, IdentifyingTypes>)this.AddChannel, channel, idTypes);
				return;
			}
			
			var parent = this.serverNode.Nodes;
			if (!Channel.IsDefault (channel.ParentChannelId, idTypes))
			{
				var pair = channelNodes.FirstOrDefault (kvp => kvp.Key.ChannelId.Equals (channel.ParentChannelId));
				if (!pair.Equals (default(KeyValuePair<Channel, TreeNode>)))
					parent = pair.Value.Nodes;
			}

			var node = parent.Add (channel.ChannelId.ToString(), channel.Name);
			node.ImageKey = "channel";
			node.SelectedImageKey = "channel";
			this.channelNodes.Add (channel, node);
		}

		public void AddUser (UserInfo user)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<UserInfo>)this.AddUser, user);
				return;
			}

			var channelPair = this.channelNodes.FirstOrDefault (c => c.Key.ChannelId.Equals (user.CurrentChannelId));
			if (channelPair.Equals (default(KeyValuePair<Channel,TreeNode>)))
				return;
			
			var node = channelPair.Value.Nodes.Add (user.Nickname);
			node.ImageKey = "silent";
			node.SelectedImageKey = "silent";
			this.userNodes[user] = node;

			node.Parent.Expand();
		}

		public void MarkTalking (UserInfo user)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<UserInfo>)this.MarkTalking, user);
				return;
			}

			if (!userNodes.ContainsKey (user) || userNodes[user].ImageKey == "talking")
				return;

			userNodes[user].ImageKey = "talking";
			userNodes[user].SelectedImageKey = "talking";
		}

		public void MarkMusic (UserInfo user)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<UserInfo>)this.MarkTalking, user);
				return;
			}

			if (!userNodes.ContainsKey (user))
				return;

			userNodes[user].ImageKey = "music";
			userNodes[user].SelectedImageKey = "music";
		}

		public void MarkSilent (UserInfo user)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<UserInfo>)this.MarkSilent, user);
				return;
			}

			if (!userNodes.ContainsKey (user) || userNodes[user].ImageKey == "silent")
				return;

			userNodes[user].ImageKey = "silent";
			userNodes[user].SelectedImageKey = "silent";
		}

		public void Update (IdentifyingTypes idTypes, IEnumerable<Channel> channels, IEnumerable<UserInfo> users)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<IdentifyingTypes, IEnumerable<Channel>, IEnumerable<UserInfo>>)Update, idTypes, channels, users);
				return;
			}

			this.channelNodes.Clear();
			this.userNodes.Clear();

			this.BeginUpdate();
			this.Nodes.Clear();
			this.serverNode.Nodes.Clear();
			this.Nodes.Add (this.serverNode);

			foreach (var channel in channels.Where (c => Channel.IsDefault (c.ParentChannelId, idTypes)))
			{
				this.AddChannel (channel, idTypes);
				this.AddChannels (idTypes, channels, channel);
			}

			this.serverNode.Expand();

			foreach (var user in users)
				this.AddUser (user);

			this.EndUpdate();
		}

		private TreeNode serverNode;
		private readonly Dictionary<Channel, TreeNode> channelNodes = new Dictionary<Channel, TreeNode>();
		private readonly Dictionary<UserInfo, TreeNode> userNodes = new Dictionary<UserInfo, TreeNode>();

		private void AddChannels (IdentifyingTypes idTypes, IEnumerable<Channel> channels, Channel parent)
		{
			foreach (var c in channels.Where (c => c.ParentChannelId.Equals (parent.ChannelId)))
			{
				this.AddChannel (c, idTypes);
				this.AddChannels (idTypes, channels, c);
			}
		}
	}
}