using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gablarski.Client;
using Gablarski.Clients.Windows.Properties;

namespace Gablarski.Clients.Windows
{
	public class UserTreeView
		: TreeView
	{
		public UserTreeView()
		{
			this.AllowDrop = true;

			this.ImageList = new ImageList
			{
				TransparentColor = Color.Transparent,
				ImageSize = new Size (16, 16),
				ColorDepth = ColorDepth.Depth24Bit
			};

			this.ImageList.Images.Add ("server",	Resources.ServerImage);
			this.ImageList.Images.Add ("channel",	Resources.ChannelImage);
			this.ImageList.Images.Add ("silent",	Resources.SoundNoneImage);
			this.ImageList.Images.Add ("talking",	Resources.SoundImage);
			this.ImageList.Images.Add ("music",		Resources.MusicImage);

			InitMenus();
		}

		public GablarskiClient Client
		{
			get; set;
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
			node.Tag = channel;
			node.ImageKey = "channel";
			node.SelectedImageKey = "channel";
			node.ContextMenuStrip = channelMenu;
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
			node.Tag = user;
			node.ImageKey = "silent";
			node.SelectedImageKey = "silent";
			node.ContextMenuStrip = userMenu;
			this.userNodes[user] = node;

			node.Parent.Expand();
		}

		public void RemoveUser (UserInfo user)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<UserInfo>)this.RemoveUser, user);
				return;
			}

			if (!this.userNodes.ContainsKey (user))
				return;

			var node = this.userNodes[user];
			node.Remove();
			this.userNodes.Remove (user);
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

		protected override void OnNodeMouseClick (TreeNodeMouseClickEventArgs e)
		{
			this.SelectedNode = e.Node;

			base.OnNodeMouseClick (e);
		}

		protected override void OnItemDrag (ItemDragEventArgs e)
		{
			var node = ((TreeNode)e.Item);
			if (node.Tag is UserInfo)
				DoDragDrop (node, DragDropEffects.Move);

			base.OnItemDrag (e);
		}

		protected override void OnDragOver (DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
			base.OnDragOver (e);
		}

		protected override void OnDragDrop (DragEventArgs e)
		{
			if (e.Data.GetDataPresent ("System.Windows.Forms.TreeNode", false))
			{
				TreeNode destinationNode = this.GetNodeAt (this.PointToClient (new Point (e.X, e.Y)));
				TreeNode movedNode = (TreeNode)e.Data.GetData ("System.Windows.Forms.TreeNode");

				if (destinationNode != null)
				{
					var channel = destinationNode.Tag as Channel;
					var user = movedNode.Tag as ClientUser;
					if (channel != null && user != null)
					{
						if (user.CurrentChannelId.Equals (channel.ChannelId))
							return;

						user.Move (channel);
					}
				}
			}

			base.OnDragDrop (e);
		}

		protected override void OnNodeMouseDoubleClick (TreeNodeMouseClickEventArgs e)
		{
			Channel channel = this.SelectedNode.Tag as Channel;
			if (e.Button != MouseButtons.Left || channel == null)
				base.OnNodeMouseDoubleClick (e);

			Client.CurrentUser.Move (channel);
		}

		private void AddChannels (IdentifyingTypes idTypes, IEnumerable<Channel> channels, Channel parent)
		{
			foreach (var c in channels.Where (c => c.ParentChannelId.Equals (parent.ChannelId)))
			{
				this.AddChannel (c, idTypes);
				this.AddChannels (idTypes, channels, c);
			}
		}

		private void ContextDeleteChannelClick (object sender, EventArgs e)
		{
			Client.Channels.Delete (this.SelectedNode.Tag as Channel);
		}

		private void ContextEditChannelClick (object sender, EventArgs e)
		{
			ChannelForm editChannel = new ChannelForm (this.SelectedNode.Tag as Channel);
			if (editChannel.ShowDialog() == DialogResult.OK)
				Client.Channels.Update (editChannel.Channel);
		}

		private void ContextAddChannelClick (object sender, EventArgs e)
		{
			ChannelForm addChannel = new ChannelForm ();
			if (addChannel.ShowDialog() == DialogResult.OK)
				Client.Channels.Create (addChannel.Channel);
		}

		private ContextMenuStrip channelMenu;
		private ContextMenuStrip userMenu;
		
		private readonly ToolStripMenuItem contextAddChannel = new ToolStripMenuItem ("Add Channel", Resources.ChannelAddImage);
		private readonly ToolStripMenuItem contextEditChannel = new ToolStripMenuItem ("Edit Channel", Resources.ChannelEditImage);
		private readonly ToolStripMenuItem contextDeleteChannel = new ToolStripMenuItem ("Delete Channel", Resources.ChannelDeleteImage);

		private void InitMenus()
		{
			if (channelMenu != null)
				return;

			this.ContextMenuStrip = new ContextMenuStrip();
			this.ContextMenuStrip.Items.Add (contextAddChannel);

			contextAddChannel.Click += ContextAddChannelClick;
			contextEditChannel.Click += ContextEditChannelClick;
			contextDeleteChannel.Click += ContextDeleteChannelClick;

			channelMenu = new ContextMenuStrip();

			channelMenu.Items.Add (contextAddChannel);
			channelMenu.Items.Add (contextEditChannel);
			channelMenu.Items.Add (contextDeleteChannel);

			userMenu = new ContextMenuStrip();
		}
	}
}