using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Gablarski.Audio;
using Gablarski.Audio.OpenAL.Providers;
using Gablarski.Client;
using Gablarski.Clients.Common;
using Gablarski.Clients.Input;
using Gablarski.Clients.Music;
using Gablarski.Clients.Windows.Entities;
using Gablarski.Clients.Windows.Properties;
using Gablarski.Messages;
using Gablarski.Network;

namespace Gablarski.Clients.Windows
{
	public partial class MainForm
		: Form
	{
		public MainForm ()
		{
			this.Icon = Resources.Headphones;

			this.gablarski = new GablarskiClient (new NetworkClientConnection());
			this.gablarski.ConnectionRejected += this.GablarskiConnectionRejected;
			this.gablarski.Connected += this.GablarskiConnected;
			this.gablarski.Disconnected += this.GablarskiDisconnected;
			this.gablarski.Channels.ReceivedChannelList += this.ChannelsReceivedChannelList;
			this.gablarski.Users.ReceivedUserList += this.UsersReceivedUserList;
			this.gablarski.Users.UserJoined += UsersUserLoggedIn;
			this.gablarski.Users.UserDisconnected += UsersUserDisconnected;
			this.gablarski.Users.UserChangedChannel += UsersUserChangedChannel;
			this.gablarski.CurrentUser.ReceivedLoginResult += this.CurrentUserReceivedLoginResult;
			this.gablarski.CurrentUser.ReceivedJoinResult += this.CurrentUserReceivedJoinResult;

			this.gablarski.Sources.ReceivedSourceList += SourcesOnReceivedSourceList;
			this.gablarski.Sources.ReceivedAudioSource += SourcesReceivedSource;
			this.gablarski.Sources.AudioSourcesRemoved += SourcesRemoved;
			this.gablarski.Sources.AudioSourceStopped += SourceStoped;
			this.gablarski.Sources.AudioSourceStarted += SourceStarted;

			this.mediaPlayerIntegration = new MediaPlayerIntegration (this.gablarski, this.gablarski.Sources,
			                                                          Settings.EnabledMediaPlayerIntegrations.Select (
			                                                          	s => (IMediaPlayer)Activator.CreateInstance (Type.GetType (s))).ToList());
			this.mediaPlayerIntegration.NormalVolume = Settings.NormalMusicVolume;
			this.mediaPlayerIntegration.TalkingVolume = Settings.TalkingMusicVolume;
			this.mediaPlayerIntegration.UserTalkingCounts = !Settings.MediaVolumeControlIgnoresYou;

			Settings.SettingChanged += SettingsSettingChanged;

			this.InitializeComponent ();

			this.users.Client = this.gablarski;

			new EventSpeech (this.gablarski, this.mediaPlayerIntegration);
		}

		private void SourcesOnReceivedSourceList(object sender, ReceivedListEventArgs<AudioSource> args)
		{
			foreach (var s in args.Data.Where (s => s.OwnerId != this.gablarski.CurrentUser.UserId))
			{
				gablarski.Audio.Attach (playback, s, new AudioEnginePlaybackOptions());
			}
		}

		public void Connect (string host, int port)
		{
			this.gablarski.Connect (host, port);
		}

		public bool ShowConnect (bool cancelExits)
		{
			var login = new LoginForm();
			if (login.ShowDialog(this) == DialogResult.OK)
			{
				if (String.IsNullOrEmpty (login.Entry.UserNickname))
				{
					InputForm nickname = new InputForm();
					if (nickname.ShowDialog() == DialogResult.Cancel)
						return false;

					string nick = nickname.Input.Text.Trim();
					login.Entry.UserNickname = nick;
				}

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
		private const string MusicName = "music";

		private IPlaybackProvider playback;
		private ICaptureProvider voiceCapture;
		private AudioSource voiceSource;
		private AudioSource musicSource;

		private void MainForm_Load (object sender, EventArgs e)
		{
			SetupInput();

			this.playback = new OpenALPlaybackProvider();
			this.playback.Device = this.playback.DefaultDevice;

			SetupVoiceCapture();
		}

		private void SetupVoiceCapture ()
		{
			try
			{
				if (this.voiceSource != null)
				{
					gablarski.Audio.Detach (this.voiceSource);
					gablarski.Sources.EndSending (voiceSource, gablarski.CurrentChannel);
				}

				if (this.voiceCapture != null)
					this.voiceCapture.Dispose();

				this.voiceCapture = (ICaptureProvider)Activator.CreateInstance (Type.GetType (Settings.VoiceProvider));

				if (String.IsNullOrEmpty (Settings.VoiceDevice))
					this.voiceCapture.Device = this.voiceCapture.DefaultDevice;
				else
				{
					this.voiceCapture.Device = this.voiceCapture.GetDevices().FirstOrDefault (d => d.Name == Settings.VoiceDevice) ??
					                           this.voiceCapture.DefaultDevice;
				}

				if (this.voiceSource != null)
				{
					gablarski.Audio.Attach (this.voiceCapture, AudioFormat.Mono16Bit, this.voiceSource, new AudioEngineCaptureOptions
					{
						StartVolume = Settings.VoiceActivationLevel,
						ContinuationVolume = Settings.VoiceActivationLevel / 2,
						ContinueThreshold = TimeSpan.FromMilliseconds (Settings.VoiceActivationContinueThreshold),
						Mode = (!Settings.UsePushToTalk) ? AudioEngineCaptureMode.Activated : AudioEngineCaptureMode.Explicit
					});
				}
			}
			catch (Exception ex)
			{
				//TaskDialog.Show (ex.ToDisplayString(), "An error as occured initializing OpenAL.", "OpenAL Initialization", TaskDialogStandardIcon.Error);
				MessageBox.Show (this, "An error occured initializing audio. " + Environment.NewLine + ex.ToDisplayString(),
				                 "OpenAL Initialization", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void Connect ()
		{
			Connect (this.server.Host, this.server.Port);
		}

		private void SettingsSettingChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "DisplaySources":
					this.users.Update (this.gablarski.Channels, this.gablarski.Users.Cast<UserInfo>(), this.gablarski.Sources);
					break;

				case Settings.UsePushToTalkSettingName:
					SetupInput();
					SetupVoiceCapture();
					break;

				case Settings.VoiceActivationLevelSettingName:
					SetupVoiceCapture();
					break;

				case Settings.VoiceActivationContinueThresholdSettingName:
					SetupVoiceCapture();
					break;

				case Settings.EnabledMediaPlayerIntegrationsSettingName:
					this.mediaPlayerIntegration.MediaPlayers = Settings.EnabledMediaPlayerIntegrations.Select (
																		s => (IMediaPlayer)Activator.CreateInstance (Type.GetType (s))).ToList ();
					break;

				case Settings.TalkingMusicVolumeSettingName:
					this.mediaPlayerIntegration.TalkingVolume = Settings.TalkingMusicVolume;
					break;

				case Settings.NormalMusicVolumeSettingName:
					this.mediaPlayerIntegration.NormalVolume = Settings.NormalMusicVolume;
					break;

				case Settings.MediaVolumeControlIgnoresYouSettingName:
					this.mediaPlayerIntegration.UserTalkingCounts = !Settings.MediaVolumeControlIgnoresYou;
					break;

				case "VoiceProvider":
					SetupVoiceCapture();
					break;

				case "VoiceDevice":
					SetupVoiceCapture();
					break;

				case "InputProvider":
					SetupInput();
					break;

				case "InputSettings":
					SetupInput();
					break;
			}
		}

		private IInputProvider inputProvider;
		private void DisableInput()
		{
			if (this.inputProvider == null)
				return;

			this.inputProvider.Detach();
			this.inputProvider.InputStateChanged -= OnInputStateChanged;
			this.inputProvider.Dispose();
			this.inputProvider = null;
		}

		private void SetupInput()
		{
			DisableInput();

			if (!Settings.UsePushToTalk)
				return;

			Type providerType = null;

			try
			{
				providerType = Type.GetType (Settings.InputProvider);
			}
			finally
			{
				if (providerType == null)
					providerType = Modules.Input.FirstOrDefault();
			}

			if (providerType == null)
				MessageBox.Show ("No input provider could be found.");
			else
			{
				this.inputProvider = (IInputProvider)Activator.CreateInstance (providerType);
				this.inputProvider.InputStateChanged += OnInputStateChanged;
				this.inputProvider.Attach (this.Handle, Settings.InputSettings);
			}
		}

		private void OnInputStateChanged (object sender, InputStateChangedEventArgs e)
		{
			if (this.voiceSource == null || this.voiceCapture == null)
				return;

			if (e.State == InputState.On)
			{
				this.users.MarkTalking (this.gablarski.CurrentUser);
				this.gablarski.Audio.BeginCapture (voiceSource, this.gablarski.CurrentChannel);
			}
			else
			{
				this.users.MarkSilent (this.gablarski.CurrentUser);
				this.gablarski.Audio.EndCapture (voiceSource, this.gablarski.CurrentChannel);
			}
		}

		void SourceStarted (object sender, AudioSourceEventArgs e)
		{
			this.users.MarkTalking (this.gablarski.Users[e.Source.OwnerId]);
		}

		void SourceStoped (object sender, AudioSourceEventArgs e)
		{
			this.users.MarkSilent (this.gablarski.Users[e.Source.OwnerId]);
		}

		void SourcesRemoved (object sender, ReceivedListEventArgs<AudioSource> e)
		{
			foreach (var s in e.Data)
				this.gablarski.Audio.Detach (s);
		}

		void SourcesReceivedSource (object sender, ReceivedAudioSourceEventArgs e)
		{
			if (e.Result == SourceResult.Succeeded)
			{
				if (e.Source.Name == VoiceName)
				{
					voiceSource = e.Source;
					gablarski.Audio.Attach (voiceCapture, AudioFormat.Mono16Bit, voiceSource,
					                        new AudioEngineCaptureOptions
					                        {
					                        	StartVolume = Settings.VoiceActivationLevel,
												ContinuationVolume = Settings.VoiceActivationLevel / 2,
												ContinueThreshold = TimeSpan.FromMilliseconds (Settings.VoiceActivationContinueThreshold),
												Mode = (!Settings.UsePushToTalk) ? AudioEngineCaptureMode.Activated : AudioEngineCaptureMode.Explicit
					                        });
				}
				else if (e.Source.Name == MusicName)
				{
					musicSource = e.Source;
					gablarski.Audio.Attach (musicprovider, AudioFormat.Mono16Bit, musicSource,
					                        new AudioEngineCaptureOptions { Mode = AudioEngineCaptureMode.Explicit });
					gablarski.Audio.BeginCapture (musicSource, gablarski.CurrentChannel);
				}

				users.Update (gablarski.Channels, gablarski.Users.Cast<UserInfo>(), gablarski.Sources);
			}
			else if (e.Result == SourceResult.NewSource)
			{
				this.gablarski.Audio.Attach (playback, e.Source, new AudioEnginePlaybackOptions ());
				users.Update (gablarski.Channels, gablarski.Users.Cast<UserInfo> (), gablarski.Sources);
			}
			else
			{
				string reason;
				switch (e.Result)
				{
					case SourceResult.FailedPermissions:
						reason = "Insufficient permissions";
						break;

					case SourceResult.FailedLimit:
						reason = "You or the server are at source capacity";
						break;

					default:
						reason = e.Result.ToString();
						break;
				}

				MessageBox.Show (this, "Source '" + e.SourceName + "' request failed: " + reason, "Source Request", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void UsersUserChangedChannel (object sender, ChannelChangedEventArgs e)
		{
			if (e.User.Equals (gablarski.CurrentUser) && musicSource != null)
			{
				gablarski.Audio.EndCapture (musicSource, e.TargetChannel);
				gablarski.Audio.BeginCapture (musicSource, e.TargetChannel);
			}

			this.users.RemoveUser (e.User);
			this.users.AddUser (e.User);
		}

		private void UsersUserDisconnected (object sender, UserEventArgs e)
		{
			this.users.RemoveUser (e.User);
		}

		private void UsersUserLoggedIn (object sender, UserEventArgs e)
		{
			this.users.AddUser (e.User);
		}

		void UsersReceivedUserList (object sender, ReceivedListEventArgs<UserInfo> e)
		{
			this.users.Update (this.gablarski.Channels, e.Data, this.gablarski.Sources);
		}

		void ChannelsReceivedChannelList (object sender, ReceivedListEventArgs<ChannelInfo> e)
		{
			this.users.Update (e.Data, this.gablarski.Users.Cast<UserInfo>(), this.gablarski.Sources);
		}

		void CurrentUserReceivedLoginResult (object sender, ReceivedLoginResultEventArgs e)
		{
			if (!e.Result.Succeeded)
			{
				Action<Form, string> d = (f, m) => MessageBox.Show (f, m);
				BeginInvoke (d, this, "Login Failed: " + e.Result.ResultState);
				gablarski.Disconnect();
			}
			else
			{
				if (LocalServer.IsRunning)
					LocalServer.Permissions.SetAdmin (e.Result.UserId);

				string serverpassword = this.server.ServerPassword;
				if (this.gablarski.ServerInfo.Passworded && serverpassword.IsEmpty())
					serverpassword = RequireServerPassword();

				if (this.gablarski.IsConnected)
					this.gablarski.CurrentUser.Join (this.server.UserNickname, serverpassword);
			}
		}

		void CurrentUserReceivedJoinResult (object sender, ReceivedJoinResultEventArgs e)
		{
			if (e.Result != LoginResultState.Success)
			{
				string reason = e.Result.ToString ();
				switch (e.Result)
				{
					case LoginResultState.FailedUsernameAndPassword:
						reason = "Username and password combination not found.";
						break;

					case LoginResultState.FailedUsername:
						reason = "The username supplied was not found or is invalid.";
						break;

					case LoginResultState.FailedServerPassword:
						reason = "The server password supplied was incorrect.";
						break;

					case LoginResultState.FailedNicknameInUse:
						reason = "The nickname supplied is already in use.";
						break;

					case LoginResultState.FailedPassword:
						reason = "The password for the username supplied was incorrect.";
						break;
				}

				Action<Form, string> d = (f, m) => MessageBox.Show (f, m);
				BeginInvoke (d, this, "Join failed: " + reason);
				gablarski.Disconnect();
			}
			else
				this.gablarski.Sources.Request ("voice", 1);
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
					new TreeNode (this.gablarski.ServerInfo.Name)
					{
						ToolTipText = this.gablarski.ServerInfo.Description
					}
				);
			});

			if (this.server == null)
				this.server = new ServerEntry ();

			string userpassword = this.server.UserPassword;

			if (this.server.UserNickname.IsEmpty ())
			{
				InputForm nickname = new InputForm ();
				if (nickname.ShowDialogOnFormThread (this) == DialogResult.Cancel)
					return;

				this.server.UserNickname = nickname.Input.Text.Trim ();
			}
			
			if (!this.server.UserName.IsEmpty() && userpassword.IsEmpty())
			{
				InputForm input = new InputForm();
				input.Input.UseSystemPasswordChar = true;
				input.Label.Text = "User Password:";
				input.Text = "Enter Password";

				if (input.ShowDialogOnFormThread (this) != DialogResult.OK)
				{
					this.gablarski.Disconnect();
					return;
				}

				userpassword = input.Input.Text;
			}

			if (this.server.UserName.IsEmpty() || this.server.UserPassword == null)
			{
				string serverpassword = this.server.ServerPassword;
				if (this.gablarski.ServerInfo.Passworded && serverpassword.IsEmpty())
					serverpassword = RequireServerPassword();

				if (this.gablarski.IsConnected)
					this.gablarski.CurrentUser.Join (this.server.UserNickname, serverpassword);
			}
			else
				this.gablarski.CurrentUser.Login (this.server.UserName, userpassword);
		}

		private string RequireServerPassword ()
		{
			InputForm input = new InputForm();
			input.Input.UseSystemPasswordChar = true;
			input.Label.Text = "Server Password:";
			input.Text = "Enter Password";

			if (input.ShowDialogOnFormThread (this) != DialogResult.OK)
			{
				this.gablarski.Disconnect();
				return null;
			}

			return input.Input.Text;
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
					BeginInvoke ((Action)(() =>
					MessageBox.Show (this, "Connecting to the server failed because it is running a newer version of Gablarski than you are.", "Connecting",
					                 MessageBoxButtons.OK, MessageBoxIcon.Warning)));
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
			LocalServer.Shutdown();
		}

		private void btnConnect_Click (object sender, EventArgs e)
		{
			if (this.gablarski.IsConnected)
			{
				this.gablarski.Disconnect();
				LocalServer.Shutdown();
			}
			else
				this.ShowConnect (false);
		}

		private void btnSettings_Click (object sender, EventArgs e)
		{
			SettingsForm settingsForm = new SettingsForm();
			settingsForm.ShowDialog();
		}

		private ICaptureProvider musicprovider;
		private readonly MediaPlayerIntegration mediaPlayerIntegration;

		private void musicButton_Click (object sender, EventArgs e)
		{
			using (MusicForm mf = new MusicForm())
			{
				if (mf.ShowDialog (this) != DialogResult.OK)
					return;
			
				if (mf.ProviderType != null)
				{
					musicprovider = (ICaptureProvider)Activator.CreateInstance (mf.ProviderType);
					musicprovider.Device = mf.CaptureDevice;
				}
				else
				{
					// TODO: decoding
				}
			}
		}
	}
}