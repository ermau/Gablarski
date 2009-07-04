using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gablarski.Client;
using Gablarski.Clients.Windows.Entities;
using Gablarski.Clients.Windows.Properties;
using Gablarski.Media.Sources;
using Gablarski.Messages;
using Gablarski.Network;
using Microsoft.WindowsAPICodePack;

namespace Gablarski.Clients.Windows
{
	public partial class MainForm
		: Form
	{
		public MainForm (ServerEntry entry)
		{
			this.server = entry;

			this.gablarski = new GablarskiClient (new ClientNetworkConnection());
			this.gablarski.ConnectionRejected += this.GablarskiConnectionRejected;
			this.gablarski.Connected += this.GablarskiConnected;
			this.gablarski.Disconnected += this.GablarskiDisconnected;
			this.gablarski.Channels.ReceivedChannelList += this.ChannelsReceivedChannelList;
			this.gablarski.Users.ReceivedUserList += this.UsersReceivedUserList;
			this.gablarski.Sources.ReceivedSource += this.SourcesReceivedSource;
			this.gablarski.CurrentUser.ReceivedLoginResult += this.CurrentUserReceivedLoginResult;

			this.InitializeComponent ();
		}

		private const string VoiceName = "voice";

		private IPlaybackProvider playbackProvider;
		private AudioSource voiceSource;

		private void MainForm_Load (object sender, EventArgs e)
		{
			try
			{
				this.playbackProvider = new OpenAL.Providers.PlaybackProvider();
				this.playbackProvider.Device = this.playbackProvider.DefaultDevice;
			}
			catch (Exception ex)
			{
				TaskDialog.Show (ex.ToDisplayString(), "An error as occured initializing OpenAL.", "OpenAL Initialization", TaskDialogStandardIcon.Error);
			}

			this.gablarski.Connect (this.server.Host, this.server.Port);
		}

		void SourcesReceivedSource (object sender, ReceivedSourceEventArgs e)
		{
			if (e.Result == SourceResult.Succeeded)
			{
				if (e.Source.OwnerId.Equals (this.gablarski.CurrentUser.UserId) && e.Source.Name == VoiceName)
					voiceSource = e.Source;
			}
			else
				TaskDialog.Show (e.Result.ToString(), "Source request failed");
		}

		void UsersReceivedUserList (object sender, ReceivedListEventArgs<UserInfo> e)
		{
			this.players.Update (this.gablarski.IdentifyingTypes, this.gablarski.Channels, e.Data);
		}

		void ChannelsReceivedChannelList (object sender, ReceivedListEventArgs<Channel> e)
		{
			this.players.Update (this.gablarski.IdentifyingTypes, e.Data, this.gablarski.Users.Cast<UserInfo>());
		}

		void CurrentUserReceivedLoginResult (object sender, ReceivedLoginResultEventArgs e)
		{
			if (!e.Result.Succeeded)
				TaskDialog.Show (e.Result.ResultState.ToString(), "Login Failed");
			else
				this.gablarski.Sources.Request ("voice", 1, 64000);
		}

		void GablarskiDisconnected (object sender, EventArgs e)
		{
			this.btnConnect.Image = Resources.DisconnectImage;
			this.btnConnect.Text = "Connect";
			this.btnConnect.ToolTipText = "Connect (Disconnected)";
		}

		void GablarskiConnected (object sender, EventArgs e)
		{
			this.btnConnect.Image = Resources.ConnectImage;
			this.btnConnect.Text = "Disconnect";
			this.btnConnect.ToolTipText = "Disconnect (Connected)";

			this.gablarski.CurrentUser.Login (this.server.UserNickname, this.server.UserName, this.server.UserPassword);
		}

		void GablarskiConnectionRejected (object sender, RejectedConnectionEventArgs e)
		{
			TaskDialog.Show (e.Reason.ToString(), "Connection rejected");
		}

		private readonly GablarskiClient gablarski;
		private readonly ServerEntry server;

		private void MainForm_FormClosing (object sender, FormClosingEventArgs e)
		{
			this.gablarski.Disconnect();
		}

		private void btnConnect_Click (object sender, EventArgs e)
		{
			if (this.gablarski.IsConnected)
				this.gablarski.Disconnect();
			else
				this.gablarski.Connect (this.server.Host, this.server.Port);
		}
	}
}