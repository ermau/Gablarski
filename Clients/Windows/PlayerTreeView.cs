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

			this.ImageList.Images.Add ("channel", Resources.UsersImage);
			this.ImageList.Images.Add ("silent", Resources.SoundNoneImage);
			this.ImageList.Images.Add ("talking", Resources.SoundImage);
			this.ImageList.Images.Add ("music", Resources.MusicImage);
		}

		public void AddChanel (Channel channel, IdentifyingTypes idTypes)
		{
			if (this.InvokeRequired)
			{
				this.Invoke ((Action<Channel, IdentifyingTypes>)this.AddChanel, channel, idTypes);
				return;
			}
			
			var parent = this.Nodes;
			if (!Channel.IsDefault (channel, idTypes))
			{
				var pair = channelNodes.FirstOrDefault (kvp => kvp.Key.ChannelId.Equals (channel.ParentChannelId));
				if (!pair.Equals (default(KeyValuePair<Channel, TreeNode>)))
					parent = pair.Value.Nodes;
			}

			var node = parent.Add (channel.ChannelId.ToString(), channel.Name);
			node.ImageKey = "channel";
			node.ExpandAll();
			this.channelNodes.Add (channel, node);
		}

		public void AddUser (UserInfo user)
		{
			if (this.InvokeRequired)
			{
				this.Invoke ((Action<UserInfo>)this.AddUser, user);
				return;
			}

			string channel = user.CurrentChannelId.ToString();

			if (this.Nodes.ContainsKey (channel))
				(this.userNodes[user] = this.Nodes[channel].Nodes.Add (user.Nickname)).ImageKey = "silent";
		}

		public void MarkTalking (UserInfo user)
		{
			if (this.InvokeRequired)
			{
				this.Invoke ((Action<UserInfo>)this.MarkTalking, user);
				return;
			}

			if (!userNodes.ContainsKey (user))
				return;

			userNodes[user].ImageKey = "talking";
		}

		public void MarkMusic (UserInfo user)
		{
			if (this.InvokeRequired)
			{
				this.Invoke ((Action<UserInfo>)this.MarkTalking, user);
				return;
			}

			if (!userNodes.ContainsKey (user))
				return;

			userNodes[user].ImageKey = "music";
		}

		public void MarkSilent (UserInfo user)
		{
			if (this.InvokeRequired)
			{
				this.Invoke ((Action<UserInfo>)this.MarkTalking, user);
				return;
			}

			if (!userNodes.ContainsKey (user))
				return;

			userNodes[user].ImageKey = "silent";
		}

		public void Update (IdentifyingTypes idTypes, IEnumerable<Channel> channels, IEnumerable<UserInfo> users)
		{
			if (this.InvokeRequired)
			{
				this.Invoke ((Action<IdentifyingTypes, IEnumerable<Channel>, IEnumerable<UserInfo>>)Update, idTypes, channels, users);
				return;
			}

			this.channelNodes.Clear();
			this.userNodes.Clear();

			this.BeginUpdate();
			this.Nodes.Clear();

			foreach (var channel in channels.Where (c => Channel.IsDefault (c.ParentChannelId, idTypes)))
			{
				this.AddChanel (channel, idTypes);
				this.AddChannels (idTypes, channels, channel);
			}

			foreach (var user in users)
				this.AddUser (user);

			this.EndUpdate();
		}

		private readonly Dictionary<Channel, TreeNode> channelNodes = new Dictionary<Channel, TreeNode>();
		private readonly Dictionary<UserInfo, TreeNode> userNodes = new Dictionary<UserInfo, TreeNode>();

		private void AddChannels (IdentifyingTypes idTypes, IEnumerable<Channel> channels, Channel parent)
		{
			foreach (var c in channels.Where (c => c.ParentChannelId.Equals (parent.ChannelId)))
			{
				this.AddChanel (c, idTypes);
				this.AddChannels (idTypes, channels, c);
			}
		}
	}
}