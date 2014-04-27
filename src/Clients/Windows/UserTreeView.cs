using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cadenza;
using Gablarski.Audio;
using Gablarski.Client;
using Gablarski.Clients.Persistence;
using Gablarski.Clients.Windows.Properties;
using Gablarski.Messages;

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
			this.ImageList.Images.Add ("connecting", Resources.HourglassImage);
			this.ImageList.Images.Add ("channel",	Resources.ChannelImage);
			this.ImageList.Images.Add ("silent",	Resources.SoundNoneImage);
			this.ImageList.Images.Add ("talking",	Resources.SoundImage);
			this.ImageList.Images.Add ("music",		Resources.MusicImage);
			this.ImageList.Images.Add ("muted",		Resources.SoundMuteImage);
			this.ImageList.Images.Add ("mutedmic",	Resources.CaptureMuteImage);
			this.ImageList.Images.Add ("afk",		Resources.UserAFKImage);
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public GablarskiClient Client
		{
			get { return this.client; }
			set
			{
				if (this.client != null)
				{
					this.client.CurrentUser.PermissionsChanged -= OnPermissionsChanged;
					this.client.Users.UserMuted -= OnUserMuted;
					this.client.Users.UserIgnored -= OnUserMuted;
					this.client.Sources.AudioSourceMuted -= OnSourceMuted;
				}

				this.client = value;
				this.client.CurrentUser.PermissionsChanged += OnPermissionsChanged;
				this.client.Users.UserMuted += OnUserMuted;
				this.client.Users.UserIgnored += OnUserMuted;
				this.client.Sources.AudioSourceMuted += OnSourceMuted;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ServerEntry Server
		{
			get;
			set;
		}

		private void OnUserMuted (object sender, UserMutedEventArgs e)
		{
			TreeNode node;
			if (!this.userNodes.TryGetValue (e.User, out node))
				return;

			if (!e.Unmuted)
				MarkMuted (e.User);
			else
				MarkSilent (e.User, true);

			SetupUserContext (node);
		}

		private void OnSourceMuted (object sender, AudioSourceMutedEventArgs e)
		{
			if (!e.Unmuted)
				MarkMuted (e.Source);
			else
				MarkSilent (e.Source);

			SetupUserContext (userNodes[this.client.Users[e.Source.OwnerId]]);
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

			if (!String.IsNullOrEmpty (node.ImageKey))
			{
				this.serverNode.ImageKey = "server";
				this.serverNode.SelectedImageKey = "server";
			}

			this.Nodes.Add (node);

			this.EndUpdate();
		}

		public void AddChannel (IChannelInfo channel)
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

		public void AddUser (IUserInfo user, IEnumerable<AudioSource> sources)
		{
			if (user == null)
				return;

			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<IUserInfo, IEnumerable<AudioSource>>)this.AddUser, user, sources);
				return;
			}

			var channelPair = this.channelNodes.FirstOrDefault (c => c.Key.ChannelId == user.CurrentChannelId);
			if (channelPair.Equals (default(KeyValuePair<ChannelInfo,TreeNode>)))
				return;

			string displayName = user.Nickname;
			if (!user.Comment.IsNullOrWhitespace())
				displayName += " (" + user.Comment + ")";

			var node = channelPair.Value.Nodes.Add (displayName);
			node.Tag = user;

			string imageKey = "silent";
			if (user.IsMuted)
				imageKey = "muted";
			else if ((user.Status & UserStatus.AFK) == UserStatus.AFK)
				imageKey = "afk";
			else if ((user.Status & UserStatus.MutedSound) == UserStatus.MutedSound)
				imageKey = "muted";
			else if ((user.Status & UserStatus.MutedMicrophone) == UserStatus.MutedMicrophone)
				imageKey = "mutedmic";
			else if (this.client.Users.GetIsIgnored (user))
				imageKey = "muted";

			node.ImageKey = node.SelectedImageKey = imageKey;

			SetupUserContext (node);

			this.userNodes[user] = node;

			if (Settings.DisplaySources)
			{
				foreach (var source in sources)
					AddSource (source);
			}

			node.Parent.Expand();
		}

		public void AddSource (AudioSource source)
		{
			if (source == null)
				return;

			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<AudioSource>)this.AddSource, source);
				return;
			}

			var user = Client.Users[source.OwnerId];
			if (user == null)
				return;

			TreeNode userNode;
			if (!this.userNodes.TryGetValue (user, out userNode))
				return;

			var snode = userNode.Nodes.Add (source.Name);
			snode.Tag = source;
			snode.SelectedImageKey = snode.ImageKey = (!source.IsMuted) ? "silent" : "muted";

			SetupSourceContext (snode);

			this.sourceNodes[source] = snode;

			userNode.Expand();
		}

		public void RemoveUser (IUserInfo user)
		{
			if (user == null)
				return;

			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<IUserInfo>)this.RemoveUser, user);
				return;
			}

			if (!this.userNodes.ContainsKey (user))
				return;

			var node = this.userNodes[user];
			node.Remove();
			this.userNodes.Remove (user);

			foreach (var kvp in this.sourceNodes.Where (kvp => kvp.Key.OwnerId == user.UserId).ToList())
				sourceNodes.Remove (kvp.Key);
		}

		public void MarkTalking (IUserInfo user)
		{
			MarkTalking (user, false);
		}

		public void MarkTalking (IUserInfo user, bool ignoreStates)
		{
			if (user == null)
				return;

			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<IUserInfo, bool>)this.MarkTalking, user, ignoreStates);
				return;
			}

			TreeNode node;
			if (!userNodes.TryGetValue (user, out node) || (!ignoreStates && NodeInState (node)))
				return;

			node.ImageKey = "talking";
			node.SelectedImageKey = "talking";
			SetupUserContext (node);
		}

		public void MarkTalking (AudioSource source)
		{
			MarkTalking (source, false);
		}

		public void MarkTalking (AudioSource source, bool ignoreStates)
		{
			if (source == null)
				return;

			IUserInfo user = client.Users[source.OwnerId];
			if (user == null)
				return;

			if (InvokeRequired) {
				BeginInvoke ((Action<AudioSource, bool>)MarkTalking, source, ignoreStates);
				return;
			}

			TreeNode node;
			if (Settings.DisplaySources && sourceNodes.TryGetValue (source, out node))
				SetupUserContext (node.Parent);
			else if (userNodes.TryGetValue (user, out node))
				SetupUserContext (node);

			if (node != null && (ignoreStates || !NodeInState (node))) {
				node.ImageKey = "talking";
				node.SelectedImageKey = "talking";
			}
		}

		public void MarkMuted (AudioSource source)
		{
			if (source == null)
				return;

			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<AudioSource>)this.MarkMuted, source);
				return;
			}

			TreeNode node;
			if (!sourceNodes.TryGetValue (source, out node))
				return;

			node.SelectedImageKey = node.ImageKey = "muted";
			SetupUserContext (node.Parent);
		}

		public void MarkMuted (IUserInfo user)
		{
			if (user == null)
				return;

			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<IUserInfo>)this.MarkMuted, user);
				return;
			}

			TreeNode node;
			if (!userNodes.TryGetValue (user, out node))
				return;

			node.SelectedImageKey = node.ImageKey = "muted";
			SetupUserContext (node);
		}

		public void MarkSilent (AudioSource source)
		{
			MarkSilent (source, false);
		}

		public void MarkSilent (AudioSource source, bool ignoreStates)
		{
			if (source == null)
				return;

			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<AudioSource, bool>)this.MarkSilent, source, ignoreStates);
				return;
			}

			TreeNode node = null;
			if (source.IsMuted)
				return;
			
			if (Settings.DisplaySources && sourceNodes.TryGetValue (source, out node))
			{
				if (ignoreStates || !NodeInState (node.Parent))
					SetupUserContext (node.Parent);
			}
			else
			{
				IUserInfo user = client.Users[source.OwnerId];
				if (user != null && userNodes.TryGetValue (user, out node) && (ignoreStates || !NodeInState (node)))
					SetupUserContext (node);
			}

			if (node != null && (ignoreStates || !NodeInState (node)))
				node.ImageKey = node.SelectedImageKey = "silent";
		}

		public void MarkSilent (IUserInfo user)
		{
			MarkSilent (user, false);
		}

		public void MarkSilent (IUserInfo user, bool ignoreStates)
		{
			if (user == null)
				return;

			if (this.InvokeRequired)
			{
				this.BeginInvoke ((Action<IUserInfo, bool>)this.MarkSilent, user, ignoreStates);
				return;
			}

			TreeNode node;
			if (!userNodes.TryGetValue (user, out node) || (!ignoreStates && NodeInState (node)))
				return;

			node.ImageKey = node.SelectedImageKey = "silent";
			SetupUserContext (node);
		}

		public void Update (IEnumerable<IChannelInfo> channels, IEnumerable<IUserInfo> users, IEnumerable<AudioSource> sources)
		{
			if (InvokeRequired)
			{
				BeginInvoke ((Action<IEnumerable<IChannelInfo>, IEnumerable<IUserInfo>, IEnumerable<AudioSource>>)Update, channels, users, sources);
				return;
			}

			this.channelNodes.Clear();
			this.userNodes.Clear();
			this.sourceNodes.Clear();

			BeginUpdate();
			this.Nodes.Clear();
			if (this.serverNode == null)
				return;
			
			this.serverNode.Nodes.Clear();
			this.Nodes.Add (this.serverNode);

			foreach (var channel in channels.Where (c => c.ParentChannelId == 0))
			{
				AddChannel (channel);
				AddChannels (channels, channel);
			}

			this.serverNode.Expand();

			foreach (var user in users)
				AddUser (user, sources.Where (s => s.OwnerId == user.UserId));

			UpdateContextMenus (false);

			EndUpdate();
		}

		private TreeNode serverNode;
		private readonly Dictionary<IChannelInfo, TreeNode> channelNodes = new Dictionary<IChannelInfo, TreeNode>();
		private readonly Dictionary<IUserInfo, TreeNode> userNodes = new Dictionary<IUserInfo, TreeNode>();
		private readonly Dictionary<AudioSource, TreeNode> sourceNodes = new Dictionary<AudioSource, TreeNode>();

		private bool NodeInState (TreeNode node)
		{
			return node.ImageKey == "muted" || node.ImageKey == "mutedmic" || node.ImageKey == "afk";
		}

		protected override void DefWndProc (ref Message m)
		{
			if (m.Msg != 515)
				base.DefWndProc (ref m);
		}

		protected override void OnNodeMouseClick (TreeNodeMouseClickEventArgs e)
		{
			this.SelectedNode = e.Node;

			base.OnNodeMouseClick (e);
		}

		protected override void OnItemDrag (ItemDragEventArgs e)
		{
			var node = ((TreeNode)e.Item);
			if (node.Tag is IUserInfo)
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
					var user = movedNode.Tag as IUserInfo;
					if (channel != null && user != null)
					{
						if (user.CurrentChannelId.Equals (channel.ChannelId))
							return;

						Client.Users.Move (user, channel);
					}
				}
			}

			base.OnDragDrop (e);
		}

		protected override void OnNodeMouseDoubleClick (TreeNodeMouseClickEventArgs e)
		{
			var node = e.Node;
			if (node.Tag == null)
				return;

			ChannelInfo channel = node.Tag as ChannelInfo;
			if (e.Button != MouseButtons.Left || channel == null)
			{
				base.OnNodeMouseDoubleClick (e);
				return;
			}

			Client.Users.Move (Client.CurrentUser, channel);
		}

		private void AddChannels (IEnumerable<IChannelInfo> channels, IChannelInfo parent)
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
			ChannelForm editChannel = new ChannelForm (this.SelectedNode.Tag as IChannelInfo);
			if (editChannel.ShowDialog() == DialogResult.OK)
				Client.Channels.Update (editChannel.Channel);
		}

		private void ContextAddChannelClick (object sender, EventArgs e)
		{
			ChannelForm addChannel = new ChannelForm ();
			if (addChannel.ShowDialog() != DialogResult.OK)
				return;

			if (this.SelectedNode != null)
			{
				var parent = (this.SelectedNode.Tag as ChannelInfo);
				if (parent != null)
					addChannel.Channel.ParentChannelId = parent.ChannelId;
			}

			this.Client.Channels.Create (addChannel.Channel);
		}

		private void ContextIgnoreUserClick (object sender, EventArgs e)
		{
			Client.Users.ToggleIgnore ((IUserInfo)this.SelectedNode.Tag);
		}

		private void ContextIgnoreSourceClick (object sender, EventArgs e)
		{
			var s = ((AudioSource)((ToolStripItem)sender).Tag);
			if (Client.Sources.ToggleIgnore (s))
				MarkMuted (s);
			else if (!s.IsMuted)
				MarkSilent (s);

			SetupUserContext (userNodes[userNodes.Keys.FirstOrDefault (u => u.UserId == s.OwnerId)]);
			if (sourceNodes.ContainsKey (s))
				SetupSourceContext (sourceNodes[s]);
		}

		private void ContextMuteUserClick (object sender, EventArgs e)
		{
			Client.Users.ToggleMute ((IUserInfo)this.SelectedNode.Tag);
		}

		private void ContextMuteSourceClick (object sender, EventArgs e)
		{
			Client.Sources.ToggleMute (((AudioSource)((ToolStripMenuItem)sender).Tag));
		}

		private GablarskiClient client;
		private void OnPermissionsChanged (object sender, EventArgs e)
		{
			UpdateContextMenus (true);
		}

		private void SetupSourceContext (TreeNode snode)
		{
			snode.ContextMenuStrip = new ContextMenuStrip();

			var target = (AudioSource)snode.Tag;

			if (target.OwnerId != Client.CurrentUser.UserId)
			{
				AddSourceContext (snode.ContextMenuStrip.Items, target);
			}
		}

		private void AddSourceContext (ToolStripItemCollection items, AudioSource source)
		{
			if (!Client.Sources.GetIsIgnored (source))
			{
				var ignore = new ToolStripMenuItem ("Ignore '" + source.Name + "'", Resources.SoundMuteImage);
				ignore.ToolTipText = "Ignores the audio source";
				ignore.Click += ContextIgnoreSourceClick;
				ignore.Tag = source;

				items.Add (ignore);
			}
			else
			{
				var ignore = new ToolStripMenuItem ("Unignore '" + source.Name + "'", Resources.SoundImage);
				ignore.ToolTipText = "Unignores the audio source";
				ignore.Click += ContextIgnoreSourceClick;
				ignore.Tag = source;

				items.Add (ignore);
			}

			if (Client.CurrentUser.Permissions.CheckPermission (PermissionName.MuteAudioSource))
			{
				if (!source.IsMuted)
				{
					var mute = new ToolStripMenuItem ("Mute '" + source.Name + "'", Resources.SoundMuteImage);
					mute.ToolTipText = "Mutes the audio source for everyone";
					mute.Click += ContextMuteSourceClick;
					mute.Tag = source;

					items.Add (mute);
				}
				else
				{
					var mute = new ToolStripMenuItem ("Unmute '" + source.Name + "'", Resources.SoundImage);
					mute.ToolTipText = "Unmutes the audio source for everyone";
					mute.Click += ContextMuteSourceClick;
					mute.Tag = source;

					items.Add (mute);
				}
			}
		}

		private void SetupUserContext (TreeNode un)
		{
			if (this.InvokeRequired)
			{
				Invoke ((Action<TreeNode>)SetupUserContext, un);
				return;
			}

			un.ContextMenuStrip = new ContextMenuStrip();
			
			var target = (IUserInfo)un.Tag;

			if (!target.Comment.IsNullOrWhitespace())
			{
				var copy = new ToolStripMenuItem ("Copy comment", Resources.CopyCommentImage);
				copy.ToolTipText = "Copys the user's comment";
				copy.Click += ContextCopyUserCommentClick;
				un.ContextMenuStrip.Items.Add (copy);

				string comment = target.Comment.Trim();
				if (comment.StartsWith ("http://") || comment.StartsWith ("https://") || comment.StartsWith ("www."))
				{
					var gotourl = new ToolStripMenuItem ("Open web page", Resources.LinkImage);
					gotourl.ToolTipText = "Opens the URL in your web browser";
					gotourl.Click += ContextGotoUrlUserCommentClick;
					un.ContextMenuStrip.Items.Add (gotourl);
				}

				un.ContextMenuStrip.Items.Add (new ToolStripSeparator());
			}

			if (!target.Equals (Client.CurrentUser))
			{
				var volume = new ToolStripMenuItem ("Adjust volume", Resources.SoundLowImage);
				volume.ToolTipText = "Allows you to set gain for the user";
				volume.Click += VolumeOnClick;
				un.ContextMenuStrip.Items.Add (volume);
			}

			ToolStripMenuItem menu = null;
			foreach (var source in Client.Sources[target])
			{
				if (menu == null)
				{
					menu = new ToolStripMenuItem ("Sources");
					un.ContextMenuStrip.Items.Add (menu);
				}

				AddSourceContext (menu.DropDown.Items, source);
			}

			un.ContextMenuStrip.Items.Add (new ToolStripSeparator());

			if (!Client.Users.GetIsIgnored (target))
			{
				var ignore = new ToolStripMenuItem ("Ignore user", Resources.SoundMuteImage);
				ignore.ToolTipText = "Ignores the user";
				ignore.Click += ContextIgnoreUserClick;

				un.ContextMenuStrip.Items.Add (ignore);
			}
			else
			{
				var ignore = new ToolStripMenuItem ("Unignore user", Resources.SoundImage);
				ignore.ToolTipText = "Unignores the user";
				ignore.Click += ContextIgnoreUserClick;

				un.ContextMenuStrip.Items.Add (ignore);
			}

			bool adminSep = false;

			if (Client.CurrentUser.Permissions.CheckPermission (PermissionName.ApproveRegistrations) && Client.ServerInfo.RegistrationMode == UserRegistrationMode.PreApproved)
			{
				if (!adminSep)
				{
					adminSep = true;
					un.ContextMenuStrip.Items.Add (new ToolStripSeparator());
				}

				var approve = new ToolStripMenuItem ("Allow registration", Resources.UserAddImage);
				approve.ToolTipText = "Allow this user to register";
				approve.Click += ContextAllowRegistration;

				un.ContextMenuStrip.Items.Add (approve);
			}

			if (this.Client.CurrentUser.Permissions.CheckPermission (PermissionName.MuteUser))
			{
				if (!adminSep)
				{
					adminSep = true;
					un.ContextMenuStrip.Items.Add (new ToolStripSeparator());
				}

				if (!target.IsMuted)
				{
					var mute = new ToolStripMenuItem ("Mute user", Resources.SoundMuteImage);
					mute.ToolTipText = "Mutes the user for everyone";
					mute.Click += ContextMuteUserClick;

					un.ContextMenuStrip.Items.Add (mute);
				}
				else
				{
					var mute = new ToolStripMenuItem ("Unmute user", Resources.SoundImage);
					mute.ToolTipText = "Unmutes the user for everyone";
					mute.Click += ContextMuteUserClick;

					un.ContextMenuStrip.Items.Add (mute);
				}
			}

			if (this.Client.CurrentUser.Permissions.CheckPermission (target.CurrentChannelId, PermissionName.KickPlayerFromChannel))
			{
				if (!adminSep)
				{
					adminSep = true;
					un.ContextMenuStrip.Items.Add (new ToolStripSeparator());
				}

				var kickChannel = new ToolStripMenuItem ("Channel kick", Resources.KickImage);
				kickChannel.ToolTipText = "Kicks this user from the channel";
				kickChannel.Click += ContextChannelKick;

				un.ContextMenuStrip.Items.Add (kickChannel);
			}

			if (this.Client.CurrentUser.Permissions.CheckPermission (PermissionName.KickPlayerFromServer))
			{
				if (!adminSep)
				{
					adminSep = true;
					un.ContextMenuStrip.Items.Add (new ToolStripSeparator());
				}

				var kickServer = new ToolStripMenuItem ("Server kick", Resources.KickImage);
				kickServer.ToolTipText = "Kicks this user from the server";
				kickServer.Click += ContextServerKick;

				un.ContextMenuStrip.Items.Add (kickServer);
			}

			if (!target.Equals (Client.CurrentUser) && this.Client.CurrentUser.Permissions.CheckPermission (PermissionName.BanUser))
			{
				if (!adminSep)
				{
					adminSep = true;
					un.ContextMenuStrip.Items.Add (new ToolStripSeparator());
				}

				var banUsername = new ToolStripMenuItem (String.Format ("Ban '{0}' ('{1}')", target.Username, target.Nickname), Resources.BanImage);
				banUsername.ToolTipText = "Bans this username from the server";
				banUsername.Click += ContextBanUsername;

				un.ContextMenuStrip.Items.Add (banUsername);
			}
		}

		private void ContextAllowRegistration (object sender, EventArgs eventArgs)
		{
			Client.Users.ApproveRegistration ((IUserInfo)this.SelectedNode.Tag);
		}

		private async void ContextBanUsername (object sender, EventArgs eventArgs)
		{
			var user = (IUserInfo)this.SelectedNode.Tag;

			await Client.Users.BanAsync (user, TimeSpan.MaxValue);
		}

		private void ContextServerKick (object sender, EventArgs e)
		{
			Client.Users.Kick ((IUserInfo)this.SelectedNode.Tag, true);
		}

		private void ContextChannelKick (object sender, EventArgs e)
		{
			Client.Users.Kick ((IUserInfo)this.SelectedNode.Tag, false);
		}

		private void VolumeOnClick (object sender, EventArgs eventArgs)
		{
			var user = (IUserInfo)this.SelectedNode.Tag;
			VolumeEntry entry = ClientData.GetVolumes (Server).FirstOrDefault (ve => ve.Username == user.Username)
				                    ?? new VolumeEntry { Username = user.Username, ServerId = Server.Id };

			VolumeForm volume = new VolumeForm (entry.Gain, v =>
			{
				foreach (var s in Client.Sources[user])
					Client.Audio.Update (s, new AudioEnginePlaybackOptions (v));
			},
			
			v =>
			{
				entry.Gain = v;
				ClientData.SaveOrUpdate (entry);
			});
			volume.ShowDialog (this.Parent);
		}

		private void ContextGotoUrlUserCommentClick(object sender, EventArgs e)
		{
			string url = ((IUserInfo) this.SelectedNode.Tag).Comment.Trim();

			if (url.StartsWith ("www."))
				url = "http://" + url;

			Process.Start (url);
		}

		private void ContextCopyUserCommentClick (object sender, EventArgs e)
		{
			Clipboard.SetText (((IUserInfo)this.SelectedNode.Tag).Comment);
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

		private void UpdateContextMenus (bool full)
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