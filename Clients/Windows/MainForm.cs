using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using Gablarski.Client;
using Gablarski.Clients.Windows.Entities;
using Gablarski.Clients.Windows.Properties;
using Gablarski.Media.Sources;
using Gablarski.Messages;
using Gablarski.Network;
using Kennedy.ManagedHooks;
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
			this.gablarski.CurrentUser.ReceivedLoginResult += this.CurrentUserReceivedLoginResult;

			this.gablarski.Sources.ReceivedSource += this.SourcesReceivedSource;
			this.gablarski.Sources.ReceivedAudio += SourcesReceivedAudio;

			Settings.SettingChanged += SettingsSettingChanged;

			this.InitializeComponent ();
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
				this.playback = new OpenAL.Providers.PlaybackProvider();
				this.playback.Device = this.playback.DefaultDevice;
				this.playback.SourceFinished += PlaybackProviderSourceFinished;

				this.voiceCapture = new OpenAL.Providers.CaptureProvider();
				this.voiceCapture.Device = this.voiceCapture.DefaultDevice;
				this.voiceCapture.SamplesAvailable += VoiceCaptureSamplesAvailable;
			}
			catch (Exception ex)
			{
				TaskDialog.Show (ex.ToDisplayString(), "An error as occured initializing OpenAL.", "OpenAL Initialization", TaskDialogStandardIcon.Error);
			}

			SetUsePushToTalk (Settings.UsePushToTalk);

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
				this.players.MarkTalking (this.gablarski.CurrentUser);
				this.voiceCapture.StartCapture();
			}
			else if (kEvent == KeyboardEvents.KeyUp)
			{
				this.players.MarkSilent (this.gablarski.CurrentUser);
				this.voiceCapture.EndCapture();
			}
		}

		private void PlaybackProviderSourceFinished (object sender, SourceFinishedEventArgs e)
		{
			this.players.MarkSilent (this.gablarski.Users[e.Source.OwnerId]);
		}

		private void SourcesReceivedAudio (object sender, ReceivedAudioEventArgs e)
		{
			this.players.MarkTalking (this.gablarski.Users[e.Source.OwnerId]);
			this.playback.QueuePlayback (e.Source, e.AudioData);
		}

		void SourcesReceivedSource (object sender, ReceivedSourceEventArgs e)
		{
			if (e.Result == SourceResult.Succeeded)
			{
				if (e.Source.OwnerId.Equals (this.gablarski.CurrentUser.UserId) && e.Source.Name == VoiceName)
					voiceSource = (ClientAudioSource)e.Source;
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
			this.players.Nodes.Clear();

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