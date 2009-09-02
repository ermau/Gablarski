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
			this.ImageList.Images.Add ("muted",		Resources.SoundMuteImage);

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

		public void AddChannel (ChannelInfo channel)
		{
			#if DEBUG
			if (channel == null)
				throw new ArgumentNullException ("channel");
			#endif

			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<ChannelInfo>)this.AddChannel, channel);
				return;
			}

			var parent = this.serverNode.Nodes;
			if (channel.ParentChannelId != 0)
			{
				var pair = channelNodes.FirstOrDefault (kvp => kvp.Key.ChannelId == channel.ParentChannelId);
				if (!pair.Equals (default(KeyValuePair<ChannelInfo, TreeNode>)))
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
			#if DEBUG
			if (user == null)
				throw new ArgumentNullException ("user");
			#endif

			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<UserInfo>)this.AddUser, user);
				return;
			}

			var channelPair = this.channelNodes.FirstOrDefault (c => c.Key.ChannelId == user.CurrentChannelId);
			if (channelPair.Equals (default(KeyValuePair<ChannelInfo,TreeNode>)))
				return;

			string imageKey = (!user.Muted) ? "silent" : "muted";

			var node = channelPair.Value.Nodes.Add (user.Nickname);
			node.Tag = user;
			node.ImageKey = imageKey;
			node.SelectedImageKey = imageKey;
			node.ContextMenuStrip = userMenu;
			this.userNodes[user] = node;

			node.Parent.Expand();
		}

		public void RemoveUser (UserInfo user)
		{
			#if DEBUG
			if (user == null)
				throw new ArgumentNullException ("user");
			#endif

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
			#if DEBUG
			if (user == null)
				throw new ArgumentNullException ("user");
			#endif

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
			#if DEBUG
			if (user == null)
				throw new ArgumentNullException ("user");
			#endif

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

		public void MarkMuted (UserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException("user");

			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<UserInfo>)this.MarkMuted, user);
				return;
			}

			if (!userNodes.ContainsKey (user))
				return;

			userNodes[user].SelectedImageKey = userNodes[user].ImageKey = "muted";
		}

		public void MarkSilent (UserInfo user)
		{
			#if DEBUG
			if (user == null)
				throw new ArgumentNullException ("user");
			#endif

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

		public void Update (IEnumerable<ChannelInfo> channels, IEnumerable<UserInfo> users)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<IEnumerable<ChannelInfo>, IEnumerable<UserInfo>>)Update, channels, users);
				return;
			}

			this.channelNodes.Clear();
			this.userNodes.Clear();

			this.BeginUpdate();
			this.Nodes.Clear();
			this.serverNode.Nodes.Clear();
			this.Nodes.Add (this.serverNode);

			foreach (var channel in channels.Where (c => c.ParentChannelId == 0))
			{
				this.AddChannel (channel);
				this.AddChannels (channels, channel);
			}

			this.serverNode.Expand();

			foreach (var user in users)
				this.AddUser (user);

			this.EndUpdate();
		}

		private TreeNode serverNode;
		private readonly Dictionary<ChannelInfo, TreeNode> channelNodes = new Dictionary<ChannelInfo, TreeNode>();
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
					var channel = destinationNode.Tag as ChannelInfo;
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
			ChannelInfo channel = this.SelectedNode.Tag as ChannelInfo;
			if (e.Button != MouseButtons.Left || channel == null)
				base.OnNodeMouseDoubleClick (e);

			Client.CurrentUser.Move (channel);
		}

		private void AddChannels (IEnumerable<ChannelInfo> channels, ChannelInfo parent)
		{
			foreach (var c in channels.Where (c => c.ParentChannelId == parent.ChannelId))
			{
				this.AddChannel (c);
				this.AddChannels (channels, c);
			}
		}

		private void ContextDeleteChannelClick (object sender, EventArgs e)
		{
			Client.Channels.Delete (this.SelectedNode.Tag as ChannelInfo);
		}

		private void ContextEditChannelClick (object sender, EventArgs e)
		{
			ChannelForm editChannel = new ChannelForm (this.SelectedNode.Tag as ChannelInfo);
			if (editChannel.ShowDialog() == DialogResult.OK)
				Client.Channels.Update (editChannel.Channel);
		}

		private void ContextAddChannelClick (object sender, EventArgs e)
		{
			ChannelForm addChannel = new ChannelForm ();
			if (addChannel.ShowDialog() == DialogResult.OK)
				Client.Channels.Create (addChannel.Channel);
		}

		private void ContextIgnoreUserClick (object sender, EventArgs e)
		{
			((ClientUser)this.SelectedNode.Tag).ToggleIgnore ();
		}

		private ContextMenuStrip channelMenu;
		private ContextMenuStrip userMenu;
		
		private readonly ToolStripMenuItem contextAddChannel = new ToolStripMenuItem ("Add Channel", Resources.ChannelAddImage);
		private readonly ToolStripMenuItem contextEditChannel = new ToolStripMenuItem ("Edit Channel", Resources.ChannelEditImage);
		private readonly ToolStripMenuItem contextDeleteChannel = new ToolStripMenuItem ("Delete Channel", Resources.ChannelDeleteImage);

		private readonly ToolStripMenuItem contextIgnoreUser = new ToolStripMenuItem ("Ignore User", Resources.SoundMuteImage);
		private readonly ToolStripMenuItem contextMuteUser = new ToolStripMenuItem ("Mute User", Resources.SoundMuteImage);

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


			contextIgnoreUser.Click += ContextIgnoreUserClick;
			
			userMenu = new ContextMenuStrip();

			userMenu.Items.Add (contextIgnoreUser);
		}
	}
}