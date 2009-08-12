using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Gablarski.Audio;
using Gablarski.Audio.OpenAL.Providers;
using Gablarski.Client;
using Gablarski.Clients.Windows.Entities;
using Gablarski.Clients.Windows.Properties;
using Gablarski.Messages;
using Gablarski.Network;
using Kennedy.ManagedHooks;
using Microsoft.WindowsAPICodePack;

namespace Gablarski.Clients.Windows
{
	public partial class MainForm
		: Form
	{
		public MainForm ()
		{
			this.gablarski = new GablarskiClient (new NetworkClientConnection());
			this.gablarski.ConnectionRejected += this.GablarskiConnectionRejected;
			this.gablarski.Connected += this.GablarskiConnected;
			this.gablarski.Disconnected += this.GablarskiDisconnected;
			this.gablarski.Channels.ReceivedChannelList += this.ChannelsReceivedChannelList;
			this.gablarski.Users.ReceivedUserList += this.UsersReceivedUserList;
			this.gablarski.Users.UserLoggedIn += UsersUserLoggedIn;
			this.gablarski.Users.UserDisconnected += UsersUserDisconnected;
			this.gablarski.Users.UserChangedChannel += UsersUserChangedChannel;
			this.gablarski.CurrentUser.ReceivedLoginResult += this.CurrentUserReceivedLoginResult;

			this.gablarski.Sources.ReceivedSource += this.SourcesReceivedSource;
			this.gablarski.Sources.ReceivedAudio += SourcesReceivedAudio;

			Settings.SettingChanged += SettingsSettingChanged;

			this.InitializeComponent ();

			this.users.Client = this.gablarski;
		}

		public bool ShowConnect (bool cancelExits)
		{
			var login = new LoginForm();
			if (login.ShowDialog(this) == DialogResult.OK)
			{
				this.server = login.Entry;
				Connect();
			}
			else if (cancelExits)
			{
				this.Close();
				return false;
			}

			return true;
		}

		private const string VoiceName = "voice";

		private PushToTalk ptt;
		private IPlaybackProvider playback;
		private ICaptureProvider voiceCapture;
		private ClientAudioSource voiceSource;

		private void MainForm_Load (object sender, EventArgs e)
		{
			try
			{
				this.playback = new OpenALPlaybackProvider();
				this.playback.Device = this.playback.DefaultDevice;
				this.playback.SourceFinished += PlaybackProviderSourceFinished;

				this.voiceCapture = new OpenALCaptureProvider();
				this.voiceCapture.Device = this.voiceCapture.DefaultDevice;
				this.voiceCapture.SamplesAvailable += VoiceCaptureSamplesAvailable;
			}
			catch (Exception ex)
			{
				//TaskDialog.Show (ex.ToDisplayString(), "An error as occured initializing OpenAL.", "OpenAL Initialization", TaskDialogStandardIcon.Error);
				MessageBox.Show (this, "An error occured initializing OpenAL" + Environment.NewLine + ex.ToDisplayString(),
				                 "OpenAL Initialization", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			SetUsePushToTalk (Settings.UsePushToTalk);
		}

		private void Connect()
		{
			this.gablarski.Connect (this.server.Host, this.server.Port);
		}

		private void VoiceCaptureSamplesAvailable (object sender, SamplesAvailableEventArgs e)
		{
			if (this.voiceSource == null || this.gablarski.CurrentUser == null)
			{
				this.voiceCapture.ReadSamples (e.Samples);
				return;
			}

			this.voiceSource.SendAudioData (this.voiceCapture.ReadSamples (this.voiceSource.FrameSize), this.gablarski.CurrentUser.CurrentChannelId);
		}

		private void SettingsSettingChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "UsePushToTalk":
					if (Settings.UsePushToTalk != (this.ptt == null))
						SetUsePushToTalk (Settings.UsePushToTalk);

					break;

				case "PushToTalk":
					this.ptt = Settings.PushToTalk;
					break;
			}
		}

		private void SetUsePushToTalk (bool use)
		{
			if (use)
			{
				Program.KHook.KeyboardEvent += KHookKeyboardEvent;
				//Program.MHook.MouseEvent += MHookMouseEvent;
				this.ptt = Settings.PushToTalk;
			}
			else
			{
				Program.KHook.KeyboardEvent -= KHookKeyboardEvent;
				//Program.MHook.MouseEvent -= MHookMouseEvent;
				this.ptt = Settings.PushToTalk;
			}
		}

		//private void MHookMouseEvent (MouseEvents mEvent, Point point)
		//{
		//    if (this.ptt.Supplier != PushToTalkSupplier.Mouse)
		//        return;
		//}

		private void KHookKeyboardEvent (KeyboardEvents kEvent, Keys key)
		{
			if (this.voiceCapture == null)
				return;

			if (this.ptt == null || this.ptt.Supplier != PushToTalkSupplier.Keyboard || this.ptt.KeyboardKeys != key)
				return;

			if (kEvent == KeyboardEvents.KeyDown)
			{
				this.users.MarkTalking (this.gablarski.CurrentUser);
				this.voiceCapture.BeginCapture();
			}
			else if (kEvent == KeyboardEvents.KeyUp)
			{
				this.users.MarkSilent (this.gablarski.CurrentUser);
				this.voiceCapture.EndCapture();
			}
		}

		private void PlaybackProviderSourceFinished (object sender, SourceFinishedEventArgs e)
		{
			var user = this.gablarski.Users[e.Source.OwnerId];
			if (user != null)
				this.users.MarkSilent (user);
		}

		private void SourcesReceivedAudio (object sender, ReceivedAudioEventArgs e)
		{
			this.users.MarkTalking (this.gablarski.Users[e.Source.OwnerId]);
			this.playback.QueuePlayback (e.Source, e.AudioData);
		}

		void SourcesReceivedSource (object sender, ReceivedSourceEventArgs e)
		{
			if (e.Result == SourceResult.Succeeded)
			{
				if (e.Source.OwnerId.Equals (this.gablarski.CurrentUser.UserId) && e.Source.Name == VoiceName)
					voiceSource = (ClientAudioSource)e.Source;
			}
			else if (e.Result != SourceResult.NewSource && e.Result != SourceResult.SourceRemoved)
				MessageBox.Show (this, e.Result.ToString());
				//TaskDialog.Show (e.Result.ToString(), "Source request failed");
		}

		private void UsersUserChangedChannel (object sender, ChannelChangedEventArgs e)
		{
			this.users.RemoveUser (e.TargetUser);
			this.users.AddUser (e.TargetUser);
		}

		private void UsersUserDisconnected (object sender, UserDisconnectedEventArgs e)
		{
			this.users.RemoveUser (e.User);
		}

		private void UsersUserLoggedIn (object sender, UserLoggedInEventArgs e)
		{
			this.users.AddUser (e.User);
		}

		void UsersReceivedUserList (object sender, ReceivedListEventArgs<UserInfo> e)
		{
			this.users.Update (this.gablarski.Channels, e.Data);
		}

		void ChannelsReceivedChannelList (object sender, ReceivedListEventArgs<ChannelInfo> e)
		{
			this.users.Update (e.Data, this.gablarski.Users.Cast<UserInfo>());
		}

		void CurrentUserReceivedLoginResult (object sender, ReceivedLoginResultEventArgs e)
		{
			if (!e.Result.Succeeded)
				//TaskDialog.Show (e.Result.ResultState.ToString(), "Login Failed");
				MessageBox.Show (this, "Login Failed" + Environment.NewLine + e.Result.ResultState);
			else
				this.gablarski.Sources.Request ("voice", 1, 64000);
		}

		void GablarskiDisconnected (object sender, EventArgs e)
		{
			this.Invoke ((Action)delegate
			{
				this.users.Nodes.Clear();

				this.btnConnect.Image = Resources.DisconnectImage;
				this.btnConnect.Text = "Connect";
				this.btnConnect.ToolTipText = "Connect (Disconnected)";
			});
		}

		private void GablarskiConnected (object sender, EventArgs e)
		{
			this.Invoke ((Action)delegate
			{
				this.btnConnect.Image = Resources.ConnectImage;
				this.btnConnect.Text = "Disconnect";
				this.btnConnect.ToolTipText = "Disconnect (Connected)";

				this.users.SetServerNode (
					new TreeNode (this.gablarski.ServerInfo.ServerName)
					{
						ToolTipText = this.gablarski.ServerInfo.ServerDescription
					}
				);
			});

			this.gablarski.CurrentUser.Login (this.server.UserNickname, this.server.UserName, this.server.UserPassword);
		}

		private void GablarskiConnectionRejected (object sender, RejectedConnectionEventArgs e)
		{
			//TaskDialog.Show (e.Reason.ToString(), "Connection rejected");
			switch (e.Reason)
			{
				case ConnectionRejectedReason.CouldNotConnect:
					if (MessageBox.Show (this, "Could not connect to the server", "Connecting", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
						Connect();
					else
						ShowConnect (true);

					break;

				case ConnectionRejectedReason.IncompatibleVersion:
					MessageBox.Show (this, "Connecting to the server failed because it is running a newer version of Gablarski than you are.", "Connecting",
					                 MessageBoxButtons.OK, MessageBoxIcon.Warning);
					break;

				default:
					MessageBox.Show (e.Reason.ToString());
					break;
			}
		}

		private readonly GablarskiClient gablarski;
		private ServerEntry server;

		private void MainForm_FormClosing (object sender, FormClosingEventArgs e)
		{
			this.gablarski.Disconnect();
		}

		private void btnConnect_Click (object sender, EventArgs e)
		{
			if (this.gablarski.IsConnected)
				this.gablarski.Disconnect();
			else
				this.ShowConnect (false);
		}

		private void btnSettings_Click (object sender, EventArgs e)
		{
			SettingsForm settingsForm = new SettingsForm();
			settingsForm.ShowDialog();
		}
	}
}