using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gablarski;
using Gablarski.Client;
using Gablarski.Media.Sources;
using Gablarski.Messages;
using Gablarski.Network;
using Gablarski.OpenAL.Providers;
using Gablarski.Server;

namespace Gablarski.Clients.Lite
{
	public partial class TestForm : Form
	{
		public TestForm ()
		{
			InitializeComponent ();
			//Trace.Listeners.Add (new TextBoxTracer (this.log));
		}

		private GuestPermissionProvider permProvider = new GuestPermissionProvider ();
		private IPlaybackProvider playback;
		private ICaptureProvider capture;
		private GablarskiClient client;
		private void connectButton_Click (object sender, EventArgs e)
		{
			string serverName = this.ServerHost.Text.Trim();
			if (String.IsNullOrEmpty (serverName))
				return;

			capture.SamplesAvailable += capture_SamplesAvailable;

			client = new GablarskiClient (new ClientNetworkConnection());
			client.Connected += client_Connected;
			client.LoginResult += client_ReceivedLogin;
			client.PlayerLoggedIn += client_ReceivedNewLogin;
			client.ReceivedSource += client_ReceivedSource;
			client.ReceivedChannelList += client_ReceivedChannelList;
			client.ReceivedChannelEditResult += client_ReceivedChannelEditResult;
			client.ReceivedPlayerList += client_ReceivedPlayerList;
			client.ReceivedSourceList += client_ReceivedSourceList;
			client.ReceivedAudioData += client_ReceivedAudioData;
			client.PlayerDisconnected += client_PlayerDisconnected;
			client.PlayerChangedChannel += client_PlayerChangedChannel;

			client.Connect (serverName, 6112);
		}

		void client_PlayerDisconnected (object sender, PlayerDisconnectedEventArgs e)
		{
			this.Invoke ((Action) delegate
          	{
				//if (this.playerNodes.ContainsKey (e.Player.PlayerId))
				this.playerNodes[e.Player.PlayerId].Remove ();
          	});
		}

		void client_ReceivedAudioData (object sender, ReceivedAudioEventArgs e)
		{
			this.playback.QueuePlayback (e.Source, e.Source.AudioCodec.Decode (e.AudioData, 44100, 10), 44100);
		}

		void capture_SamplesAvailable (object sender, SamplesAvailableEventArgs e)
		{
			if (this.source != null)
				this.client.SendAudioData ((Channel)this.channelNodes[this.client.Self.CurrentChannelId].Tag, this.source, capture.ReadSamples (128));
		}

		void client_ReceivedNewLogin (object sender, ReceivedLoginEventArgs e)
		{
			this.Invoke ((Action) delegate
          	{
				this.playerNodes[e.PlayerInfo.PlayerId] = this.channelNodes[e.PlayerInfo.CurrentChannelId].Nodes.Add (e.PlayerInfo.Nickname);
          	});
		}

		void client_ReceivedSourceList (object sender, ReceivedListEventArgs<MediaSourceInfo> e)
		{
			this.Invoke ((Action)delegate
			{
				this.playerList.BeginUpdate ();
				foreach (var s in this.sourceNodes.Values)
					s.Remove ();

				this.sourceNodes.Clear ();

				foreach (var s in e.Data)
				{
					if (!this.playerNodes.ContainsKey (s.PlayerId))
						continue;

					this.sourceNodes.Add (s.SourceId, this.playerNodes[s.PlayerId].Nodes.Add (s.SourceTypeName));
				}

				this.playerList.EndUpdate ();
			});
		}

		void client_PlayerChangedChannel (object sender, ChannelChangedEventArgs e)
		{
			this.Invoke ((Action)delegate
			{
				if (!this.playerNodes.ContainsKey (e.MoveInfo.TargetPlayerId) || !this.channelNodes.ContainsKey (e.MoveInfo.TargetChannelId))
					return;

				var node = this.playerNodes[e.MoveInfo.TargetPlayerId];
				node.Remove();

				this.channelNodes[e.MoveInfo.TargetChannelId].Nodes.Add (node);
			});
		}

		void client_ReceivedChannelList (object sender, ReceivedListEventArgs<Channel> e)
		{
			this.Invoke ((Action)delegate
			{
				this.playerList.BeginUpdate ();
				this.playerList.Nodes.Clear ();
				this.channelNodes.Clear ();

				foreach (Channel channel in e.Data)
				{
					if (this.channelNodes.ContainsKey (channel.ChannelId))
					    continue;

					SetupChannelTree (e.Data, channel);
				}

				this.playerList.EndUpdate ();

				this.AddPlayers (this.client.Players);

				this.playerList.ExpandAll ();
			});
		}

		private void SetupChannelTree (IEnumerable<Channel> channels, Channel chan)
		{
			if (chan == null)
				return;

			if (chan.ParentChannelId != 0 && !this.channelNodes.ContainsKey (chan.ParentChannelId))
				SetupChannelTree (channels, channels.Where (c => c.ChannelId == chan.ParentChannelId).FirstOrDefault ());

			this.channelNodes.Add (chan.ChannelId, ((chan.ParentChannelId == 0) ? this.playerList.Nodes : this.channelNodes[chan.ParentChannelId].Nodes).Add (chan.Name));
			this.channelNodes[chan.ChannelId].Tag = chan;
		}

		private void AddPlayers (IEnumerable<PlayerInfo> players)
		{
			this.playerList.BeginUpdate ();

			foreach (var node in this.playerNodes.Values)
				node.Remove ();

			this.playerNodes.Clear ();

			foreach (var p in players)
			{
				if (!this.channelNodes.ContainsKey (p.CurrentChannelId))
					continue;

				this.playerNodes[p.PlayerId] = this.channelNodes[p.CurrentChannelId].Nodes.Add (p.Nickname);
				this.channelNodes[p.CurrentChannelId].Expand ();
			}

			this.playerList.EndUpdate ();
		}

		void client_ReceivedPlayerList (object sender, ReceivedListEventArgs<PlayerInfo> e)
		{
			this.Invoke ((Action<IEnumerable<PlayerInfo>>)AddPlayers, e.Data);
		}

		private Dictionary<long, TreeNode> channelNodes = new Dictionary<long, TreeNode> ();
		private Dictionary<long, TreeNode> playerNodes = new Dictionary<long, TreeNode> ();
		private Dictionary<int, TreeNode> sourceNodes = new Dictionary<int, TreeNode> ();

		void client_ReceivedSource (object sender, ReceivedSourceEventArgs e)
		{
			if (e.Result == SourceResult.Succeeded)
			{
				this.Invoke ((Action)delegate
								{
									this.requestSource.Enabled = false;
									this.sourceSelect.Items.Add (e.Source);
									this.sourceSelect.Enabled = true;
									this.transmit.Enabled = true;
								});
			}
			
			if (e.Result == SourceResult.NewSource || e.Result == SourceResult.Succeeded)
			{
				this.Invoke ((Action)delegate
				{
					if (!this.playerNodes.ContainsKey (e.SourceInfo.PlayerId))
						return;

					this.sourceNodes[e.SourceInfo.SourceId] = this.playerNodes[e.SourceInfo.PlayerId].Nodes.Add (e.SourceInfo.SourceTypeName);
					this.playerNodes[e.SourceInfo.PlayerId].Expand ();
				});
			}
			else if (e.Result == SourceResult.SourceRemoved)
			{
				this.Invoke ((Action)delegate
				{
					if (!this.sourceNodes.ContainsKey (e.SourceInfo.SourceId))
						return;

					this.sourceNodes[e.SourceInfo.SourceId].Remove ();
					this.sourceNodes.Remove (e.SourceInfo.SourceId);
				});
			}
			else
				MessageBox.Show (e.Result.ToString ());
		}

		void client_ReceivedLogin (object sender, ReceivedLoginEventArgs e)
		{
			if (!e.Result.Succeeded)
			{
				MessageBox.Show (e.Result.ResultState.ToString());
				return;
			}

			permProvider.SetAdmin (e.Result.PlayerId);

			this.Invoke ((Action)delegate
			{
				this.sourceRequestSelect.Enabled = true;
				this.requestSource.Enabled = true;
			});
		}

		void client_Connected (object sender, EventArgs e)
		{
			this.nickname.Enabled = true;
			this.login.Enabled = true;
		}

		private void TestForm_Load (object sender, EventArgs e)
		{
			this.userProviderSelect.Items.Add (typeof (GuestUserProvider));
			this.userProviderSelect.SelectedIndex = 0;

			this.playbackProviderSelect.Items.Add (typeof (PlaybackProvider));
			this.playbackProviderSelect.SelectedIndex = 0;

			this.captureProviderSelect.Items.Add (typeof (CaptureProvider));
			this.captureProviderSelect.SelectedIndex = 0;

			this.sourceRequestSelect.Items.Add (typeof (VoiceSource));
			this.sourceRequestSelect.SelectedIndex = 0;
		}

		private void playbackProviderSelect_SelectedIndexChanged (object sender, EventArgs e)
		{
			this.outputSelect.Items.Clear();

			object providerType = this.playbackProviderSelect.SelectedItem;
			if (providerType == null)
				return;

			this.playback = (IPlaybackProvider) Activator.CreateInstance ((Type) providerType);
			foreach (IDevice device in this.playback.GetDevices ())
				this.outputSelect.Items.Add (device);

			this.outputSelect.SelectedItem = this.playback.DefaultDevice;
		}

		private void outputSelect_SelectedIndexChanged (object sender, EventArgs e)
		{
			object selected = this.outputSelect.SelectedItem;
			if (selected == null)
				return;

			this.playback.Device = (IDevice)selected;
		}

		private void captureProviderSelect_SelectedIndexChanged (object sender, EventArgs e)
		{
			this.inputSelect.Items.Clear();

			object providerType = this.captureProviderSelect.SelectedItem;
			if (providerType == null)
				return;

			this.capture = (ICaptureProvider)Activator.CreateInstance ((Type)providerType);
			foreach (IDevice device in this.capture.GetDevices ())
				this.inputSelect.Items.Add (device);

			this.inputSelect.SelectedItem = this.capture.DefaultDevice;
		}

		private void inputSelect_SelectedIndexChanged (object sender, EventArgs e)
		{
			object selected = this.inputSelect.SelectedItem;
			if (selected == null)
				return;

			this.capture.Device = (IDevice)selected;
		}

		private class TextBoxTracer
			: TraceListener
		{
			public TextBoxTracer (TextBox box)
			{
				this.textbox = box;
			}

			private readonly TextBox textbox;
			public override void Write (string message)
			{
				if (this.textbox.InvokeRequired)
				{
					this.textbox.Invoke ((Action<string>) this.Write, message);
					return;
				}

				if (!this.textbox.Disposing)
					this.textbox.Text += message;
			}

			public override void WriteLine (string message)
			{
				if (this.textbox.InvokeRequired)
				{
					this.textbox.Invoke ((Action<string>) this.WriteLine, message);
					return;
				}

				if (!this.textbox.Disposing)
					this.textbox.Text += message + Environment.NewLine;
			}
		}

		private GablarskiServer server;
		private void startServerButton_Click (object sender, EventArgs e)
		{
			var channels = new LobbyChannelProvider();
			channels.SaveChannel (new Channel { Name = "Sub Test 2", Description = "Testing Subchannels", ParentChannelId = 4 });
			channels.SaveChannel (new Channel { Name = "Test 1", Description = "Testing Channel" });
			channels.SaveChannel (new Channel { Name = "Sub Test 1", Description = "Testing Subchannels", ParentChannelId = 1 });

			server = new GablarskiServer (new ServerSettings { Name = this.ServerName.Text }, new GuestUserProvider(), permProvider, channels);
			server.Start ();
			server.AddConnectionProvider (new ServerNetworkConnectionProvider());
		}

		private void login_Click (object sender, EventArgs e)
		{
			string nick = this.nickname.Text.Trim();
			if (String.IsNullOrEmpty (nick))
				return;

			string uname = this.username.Text.Trim ();
			if (String.IsNullOrEmpty (uname))
				return;

			string pword = this.password.Text.Trim ();
			if (String.IsNullOrEmpty (pword))
				return;

			this.client.Login (nick, uname, pword);
		}

		private void requestSource_Click (object sender, EventArgs e)
		{
			Type sourceType = (this.sourceRequestSelect.SelectedItem as Type);
			if (sourceType == null)
				return;

			this.client.RequestSource (sourceType, 1);
		}

		private bool capturing = false;
		private void transmit_Click (object sender, EventArgs e)
		{
			if (!capturing)
			{
				this.capture.StartCapture();
				capturing = true;
			}
			else
			{
				this.capture.EndCapture ();
				capturing = false;
			}
		}

		private IMediaSource source;
		private void sourceSelect_SelectedIndexChanged (object sender, EventArgs e)
		{
			this.source = (this.sourceSelect.SelectedItem as IMediaSource);
			this.transmit.Enabled = (this.source != null);
		}

		private void TestForm_FormClosed (object sender, FormClosedEventArgs e)
		{
			if (this.client != null)
				this.client.Disconnect();

			if (this.server != null)
				this.server.Shutdown ();
		}

		private void sourceRequestSelect_SelectedIndexChanged (object sender, EventArgs e)
		{
			if (this.source != null)
				this.sourceRequestSelect.Enabled = this.sourceSelect.Items.Contains (this.source);
		}

		private void playerList_NodeMouseDoubleClick (object sender, TreeNodeMouseClickEventArgs e)
		{
			if (!this.channelNodes.ContainsValue (e.Node))
				return;

			Channel channel = (Channel)this.channelNodes.Where (kvp => kvp.Value == e.Node).First ().Value.Tag;
			this.client.MovePlayerToChannel (this.client.Self, channel);

			e.Node.Expand ();
		}

		private void playerList_AfterSelect (object sender, TreeViewEventArgs e)
		{
			if (e.Node != null)
			{
				var channel = (e.Node.Tag as Channel);
				if (channel != null)
				{
					this.channelName.Text = channel.Name;
					this.channelDescription.Text = channel.Description;
					this.btnUpdateChannel.Text = "Update";
					return;
				}
			}

			this.btnUpdateChannel.Text = "Create";
			this.channelName.Clear ();
			this.channelDescription.Clear ();
		}

		private void btnUpdateChannel_Click (object sender, EventArgs e)
		{
			if (this.playerList.SelectedNode != null)
			{
				var channel = (this.playerList.SelectedNode.Tag as Channel);
				if (channel != null)
				{
					channel.Name = this.channelName.Text.Trim();
					channel.Description = this.channelDescription.Text.Trim();
					this.client.EditChannel (channel);

					return;
				}
			}
			
			this.client.CreateChannel (new Channel { Name = this.channelName.Text.Trim (), Description = this.channelDescription.Text.Trim () });
		}

		private void client_ReceivedChannelEditResult (object sender, ChannelEditResultEventArgs e)
		{
			if (e.Result != ChannelEditResult.Success)
				MessageBox.Show (e.Result.ToString());
		}
	}
}
