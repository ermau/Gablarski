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
		}

		public GablarskiClient Client
		{
			get { return this.client; }
			set
			{
				if (this.client != null)
				{
					this.client.CurrentUser.PermissionsChanged -= OnPermissionsChanged;
					this.client.Users.UserMuted -= OnUserMuted;
				}

				this.client = value;
				this.client.CurrentUser.PermissionsChanged += OnPermissionsChanged;
				this.client.Users.UserMuted += OnUserMuted;
			}
		}

		private void OnUserMuted (object sender, UserEventArgs e)
		{
			MarkMuted (e.User);
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
			if (channel == null)
				return;

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

			SetupChannelContext (node);

			this.channelNodes.Add (channel, node);
		}

		public void AddUser (UserInfo user)
		{
			if (user == null)
				return;

			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<UserInfo>)this.AddUser, user);
				return;
			}

			var channelPair = this.channelNodes.FirstOrDefault (c => c.Key.ChannelId == user.CurrentChannelId);
			if (channelPair.Equals (default(KeyValuePair<ChannelInfo,TreeNode>)))
				return;

			string imageKey = (!user.IsMuted) ? "silent" : "muted";

			var node = channelPair.Value.Nodes.Add (user.Nickname);
			node.Tag = user;
			node.ImageKey = imageKey;
			node.SelectedImageKey = imageKey;

			SetupUserContext (node);

			this.userNodes[user] = node;

			node.Parent.Expand();
		}

		public void RemoveUser (UserInfo user)
		{
			if (user == null)
				return;

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
			if (user == null)
				return;

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

			userNodes[user].ImageKey = userNodes[user].SelectedImageKey = "music";
		}

		public void MarkMuted (UserInfo user)
		{
			if (user == null)
				return;

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
			if (user == null)
				return;

			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<UserInfo>)this.MarkSilent, user);
				return;
			}

			if (!userNodes.ContainsKey (user) || user.IsMuted || userNodes[user].ImageKey == "silent" || userNodes[user].ImageKey == "muted")
				return;

			userNodes[user].ImageKey = userNodes[user].SelectedImageKey = "silent";
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

			UpdateContextMenus (false);

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
			{
				base.OnNodeMouseDoubleClick (e);
				return;
			}

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
			var u = (ClientUser)this.SelectedNode.Tag;
			if (u.ToggleIgnore ())
				MarkMuted (u);
			else if (!u.IsMuted)
				MarkSilent (u);
		}

		private void ContextMuteUserClick (object sender, EventArgs e)
		{
			((ClientUser)this.SelectedNode.Tag).ToggleMute();
		}

		private GablarskiClient client;
		private void OnPermissionsChanged (object sender, EventArgs e)
		{
			UpdateContextMenus (true);
		}

		private void SetupUserContext (TreeNode un)
		{
			un.ContextMenuStrip = new ContextMenuStrip();
			
			var ignore = new ToolStripMenuItem ("Ignore user", Resources.SoundMuteImage);
			ignore.ToolTipText = "Ignores the user";
			ignore.Click += ContextIgnoreUserClick;
			
			un.ContextMenuStrip.Items.Add (ignore);

			if (this.Client.CurrentUser.Permissions.CheckPermission (PermissionName.MuteUser))
			{
				var mute = new ToolStripMenuItem ("Mute user", Resources.SoundMuteImage);
				mute.ToolTipText = "Mutes the user for everyone";
				mute.Click += ContextMuteUserClick;

				un.ContextMenuStrip.Items.Add (mute);
			}
		}
		
		private void SetupChannelContext (TreeNode cn)
		{
			var channel = (ChannelInfo)cn.Tag;
			cn.ContextMenuStrip = new ContextMenuStrip();

			if (this.client.CurrentUser.Permissions.CheckPermission (PermissionName.AddChannel))
			{
				var add = new ToolStripMenuItem ("Add Channel", Resources.ChannelAddImage);
				add.Click += ContextAddChannelClick;

				cn.ContextMenuStrip.Items.Add (add);
			}

			if (!channel.ReadOnly && this.Client.CurrentUser.Permissions.CheckPermission (PermissionName.EditChannel))
			{
				var edit = new ToolStripMenuItem ("Edit Channel", Resources.ChannelEditImage);
				edit.Click += ContextEditChannelClick;

				cn.ContextMenuStrip.Items.Add (edit);
			}

			if (!channel.ReadOnly && this.Client.CurrentUser.Permissions.CheckPermission (PermissionName.DeleteChannel))
			{
				var delete = new ToolStripMenuItem ("Delete Channel", Resources.ChannelDeleteImage);
				delete.Click += ContextDeleteChannelClick;

				cn.ContextMenuStrip.Items.Add (delete);
			}
		}

		private void UpdateContextMenus(bool full)
		{
			this.ContextMenuStrip = new ContextMenuStrip();
			
			if (this.client.CurrentUser.Permissions.CheckPermission (PermissionName.AddChannel))
			{
				var add = new ToolStripMenuItem ("Add Channel", Resources.ChannelAddImage);
				add.Click += ContextAddChannelClick;

				this.ContextMenuStrip.Items.Add (add);
			}

			if (full)
			{
				foreach (var cn in this.channelNodes.Values)
					SetupChannelContext (cn);

				foreach (var un in this.userNodes.Values)
					SetupUserContext (un);
			}
		}		
	}
}