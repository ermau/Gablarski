using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Gablarski.Audio;
using Gablarski.Client;
using Gablarski.Clients.Input;
using Gablarski.Clients.Media;
using Gablarski.Clients.Windows.Entities;
using Gablarski.Clients.Windows.Properties;
using Gablarski.Messages;
using Gablarski.Network;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.IO;
using System.Diagnostics;
using Cadenza;

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
			this.gablarski.Users.UserJoined += UsersUserJoined;
			this.gablarski.Users.UserDisconnected += UsersUserDisconnected;
			this.gablarski.Users.UserChangedChannel += UsersUserChangedChannel;
			this.gablarski.Users.UserUpdated += UsersUserUpdated;
			this.gablarski.CurrentUser.ReceivedLoginResult += this.CurrentUserReceivedLoginResult;
			this.gablarski.CurrentUser.ReceivedJoinResult += this.CurrentUserReceivedJoinResult;

			this.gablarski.Sources.ReceivedSourceList += SourcesOnReceivedSourceList;
			this.gablarski.Sources.ReceivedAudioSource += SourcesReceivedSource;
			this.gablarski.Sources.AudioSourcesRemoved += SourcesRemoved;
			this.gablarski.Sources.AudioSourceStopped += SourceStoped;
			this.gablarski.Sources.AudioSourceStarted += SourceStarted;
			
			IEnumerable<IMediaPlayer> players = Enumerable.Empty<IMediaPlayer>();
			if (Settings.EnableMediaVolumeControl)
				players = Settings.EnabledMediaPlayerIntegrations.Select (s => (IMediaPlayer)Activator.CreateInstance (Type.GetType (s))).ToList ();

			this.mediaPlayerIntegration = new MediaController (this.gablarski, this.gablarski.Sources, players)
			{
				NormalVolume = Settings.NormalMusicVolume,
				TalkingVolume = Settings.TalkingMusicVolume,
				UserTalkingCounts = !Settings.MediaVolumeControlIgnoresYou
			};

			if (Settings.EnableNotifications)
				SetupNotifications ();

			Settings.SettingChanged += SettingsSettingChanged;

			this.InitializeComponent ();

			this.users.Client = this.gablarski;

			SetupInput ();

			SetupPlayback ();
			SetupVoiceCapture ();
		}

		void UsersUserUpdated (object sender, UserEventArgs e)
		{
			this.users.RemoveUser (e.User);
			this.users.AddUser (e.User, gablarski.Sources[e.User]);
		}

		private void SetupNotifications ()
		{
			this.notifications = new NotificationHandler (this.gablarski);
			this.notifications.MediaController = this.mediaPlayerIntegration;
			this.notifications.Notifiers = Settings.EnabledNotifiers.Select (n => ((INotifier)Activator.CreateInstance (Type.GetType (n))));
		}

		private void SourcesOnReceivedSourceList(object sender, ReceivedListEventArgs<AudioSource> args)
		{
			gablarski.Audio.Attach (playback, args.Data.Where (s => s.OwnerId != this.gablarski.CurrentUser.UserId), new AudioEnginePlaybackOptions());
		}

		public void Connect (string host, int port)
		{
			this.gablarski.Connect (host, port);
		}

		public void Connect (ServerEntry connectTo)
		{
			if (String.IsNullOrEmpty (connectTo.UserNickname))
			{
				InputForm nickname = new InputForm();
				if (nickname.ShowDialog () == DialogResult.Cancel)
					return;

				string nick = nickname.Input.Text.Trim();
				connectTo.UserNickname = nick;
			}

			this.server = connectTo;
			Connect ();
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

		private void SetupPlayback ()
		{
			if (this.playback != null)
			{
				this.gablarski.Audio.Detach (this.playback);
				this.playback.Dispose ();
			}

			this.playback = (IPlaybackProvider)Activator.CreateInstance (Type.GetType (Settings.PlaybackProvider));

			if (Settings.PlaybackDevice.IsNullOrWhitespace ())
				this.playback.Device = this.playback.DefaultDevice;
			else
			{
				this.playback.Device = this.playback.GetDevices ().FirstOrDefault (d => d.Name == Settings.PlaybackDevice) ??
										this.playback.DefaultDevice;
			}

			this.playback.Open();
			this.gablarski.Audio.Attach (this.playback, this.gablarski.Sources, new AudioEnginePlaybackOptions());
		}

		private void SetupVoiceCapture ()
		{
			try
			{
				foreach (var source in gablarski.Sources.Where (s => s.OwnerId == gablarski.CurrentUser.UserId))
					gablarski.Audio.EndCapture (source);

				if (this.voiceCapture != null)
				{
					gablarski.Audio.Detach (this.voiceCapture);
					this.voiceCapture.Dispose ();
				}

				if (this.musicprovider != null)
				{
					gablarski.Audio.Detach (this.musicprovider);
					this.musicprovider.Dispose ();
				}

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
				//TaskDialog.Show (ex.ToDisplayString(), "An error as occured initializing capture.", "Capture Initialization", TaskDialogStandardIcon.Error);
				MessageBox.Show (this, "An error occured initializing capture. " + Environment.NewLine + ex.ToDisplayString(),
				                 "Capture Initialization", MessageBoxButtons.OK, MessageBoxIcon.Error);

				btnMuteMic.Checked = true;
				btnMuteMic.Enabled = false;
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

				case Settings.EnableMediaVolumeControlSettingName:
				case Settings.EnabledMediaPlayerIntegrationsSettingName:		
					IEnumerable<IMediaPlayer> players = Enumerable.Empty<IMediaPlayer>();
					if (Settings.EnableMediaVolumeControl)
						players = Settings.EnabledMediaPlayerIntegrations.Select (s => (IMediaPlayer)Activator.CreateInstance (Type.GetType (s))).ToList ();

					this.mediaPlayerIntegration.MediaPlayers = players;
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

				case Settings.EnableNotificationsSettingName:
					if (!Settings.EnableNotifications)
					{
						this.notifications.Close ();
						this.notifications = null;
					}
					else
						SetupNotifications ();

					break;

				case Settings.EnabledNotifiersSettingName:
					if (Settings.EnableNotifications)
						this.notifications.Notifiers = Settings.EnabledNotifiers.Select (t => (INotifier)Activator.CreateInstance (Type.GetType (t))).ToList();

					break;

				case Settings.PlaybackProviderSettingName:
					SetupPlayback ();
					break;

				case Settings.PlaybackDeviceSettingName:
					SetupPlayback ();
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

			Type settingType;
			if (Settings.InputProvider.IsNullOrWhitespace() || (settingType = Type.GetType (Settings.InputProvider)) == null)
				this.inputProvider = Modules.Input.FirstOrDefault();
			else
				this.inputProvider = (IInputProvider)Activator.CreateInstance (settingType);

			if (this.inputProvider == null)
				Settings.UsePushToTalk = false;
			else
			{
				this.inputProvider.InputStateChanged += OnInputStateChanged;
				this.inputProvider.Attach (this.Handle, Settings.InputSettings);
			}
		}

		private void OnInputStateChanged (object sender, InputStateChangedEventArgs e)
		{
			if (this.voiceSource == null || this.voiceCapture == null)
				return;

			if (e.State == InputState.On)
				this.gablarski.Audio.BeginCapture (voiceSource, this.gablarski.CurrentChannel);
			else
				this.gablarski.Audio.EndCapture (voiceSource);
		}

		void SourceStarted (object sender, AudioSourceEventArgs e)
		{
			this.users.MarkTalking (this.gablarski.Users[e.Source.OwnerId]);
			this.users.MarkTalking (e.Source);
		}

		void SourceStoped (object sender, AudioSourceEventArgs e)
		{
			this.users.MarkSilent (this.gablarski.Users[e.Source.OwnerId]);
			this.users.MarkSilent (e.Source);
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

				users.Update (gablarski.Channels, gablarski.Users, gablarski.Sources);
			}
			else if (e.Result == SourceResult.NewSource)
			{
				this.gablarski.Audio.Attach (playback, e.Source, new AudioEnginePlaybackOptions ());

				var user = gablarski.Users[e.Source.OwnerId];
				users.RemoveUser (user);
				users.AddUser (user, gablarski.Sources[user]);
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
				gablarski.Audio.EndCapture (musicSource);
				gablarski.Audio.BeginCapture (musicSource, e.TargetChannel);
			}

			this.users.RemoveUser (e.User);
			this.users.AddUser (e.User, gablarski.Sources[e.User]);
		}

		private void UsersUserDisconnected (object sender, UserEventArgs e)
		{
			this.users.RemoveUser (e.User);
		}

		private void UsersUserJoined (object sender, UserEventArgs e)
		{
			this.users.AddUser (e.User, Enumerable.Empty<AudioSource>());
		}

		void UsersReceivedUserList (object sender, ReceivedListEventArgs<UserInfo> e)
		{
			this.users.Update (this.gablarski.Channels, e.Data, this.gablarski.Sources);
		}

		void ChannelsReceivedChannelList (object sender, ReceivedListEventArgs<ChannelInfo> e)
		{
			this.users.Update (e.Data, this.gablarski.Users, this.gablarski.Sources);
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
				if (this.gablarski.ServerInfo.Passworded && serverpassword.IsNullOrWhitespace())
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
						reason = "This server requires a user login.";
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
				this.gablarski.Sources.Request ("voice", 1, 512);
		}

		void GablarskiDisconnected (object sender, EventArgs e)
		{
			if (this.IsDisposed || this.Disposing || this.shuttingDown)
				return;

			this.Invoke ((Action)delegate
			{
				if (this.IsDisposed || this.Disposing)
					return;

				if (TaskbarManager.IsPlatformSupported)
				{
					TaskbarManager.Instance.SetOverlayIcon (this.Handle, Resources.DisconnectImage.ToIcon (), "Disconnected");
				}

				this.users.Nodes.Clear();

				this.btnConnect.Image = Resources.DisconnectImage;
				this.btnConnect.Text = "Connect";
				this.btnConnect.ToolTipText = "Connect (Disconnected)";
			});
		}

		private void GablarskiConnected (object sender, EventArgs e)
		{
			if (this.IsDisposed || this.Disposing)
				return;

			this.Invoke ((Action)delegate
			{
				if (TaskbarManager.IsPlatformSupported)
					TaskbarManager.Instance.SetOverlayIcon (this.Handle, Resources.ConnectImage.ToIcon (), "Connected");

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
				this.server = new ServerEntry (0);

			string userpassword = this.server.UserPassword;

			if (this.server.UserNickname.IsNullOrWhitespace ())
			{
				InputForm nickname = new InputForm ();
				if (nickname.ShowDialogOnFormThread (this) == DialogResult.Cancel)
					return;

				this.server.UserNickname = nickname.Input.Text.Trim ();
			}
			
			if (!this.server.UserName.IsNullOrWhitespace() && userpassword.IsNullOrWhitespace())
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

			if (this.server.UserName.IsNullOrWhitespace() || this.server.UserPassword == null)
			{
				string serverpassword = this.server.ServerPassword;
				if (this.gablarski.ServerInfo.Passworded && serverpassword.IsNullOrWhitespace())
					serverpassword = RequireServerPassword();

				if (this.gablarski.IsConnected)
					this.gablarski.CurrentUser.Join (this.server.UserNickname, this.server.UserPhonetic, serverpassword);
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
					BeginInvoke ((Action)(() =>
					{
						if (
							MessageBox.Show (this, "Could not connect to the server", "Connecting", MessageBoxButtons.RetryCancel,
							                 MessageBoxIcon.Warning) == DialogResult.Retry)
							Connect();
						else
							ShowConnect (true);
					}));

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
		private bool shuttingDown;

		private void MainForm_FormClosing (object sender, FormClosingEventArgs e)
		{
			this.shuttingDown = true;

			if (this.notifications != null)
				this.notifications.Close ();

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
		private readonly MediaController mediaPlayerIntegration;
		private NotificationHandler notifications;

		private void musicButton_Click (object sender, EventArgs e)
		{
			using (MusicForm mf = new MusicForm())
			{
				if (mf.ShowDialog (this) != DialogResult.OK)
					return;
			
				if (!mf.File && mf.Provider != null)
				{
					musicprovider = (ICaptureProvider)mf.Provider;
					musicprovider.Device = mf.CaptureDevice;
				}
				else
				{
					//musicprovider = new MusicFileCaptureProvider (new FileInfo (mf.FilePath));
				}
			}
		}

		private void btnMute_Click (object sender, EventArgs e)
		{
			if (gablarski == null)
				return;

			if (this.btnMute.Checked)
			{
				if (!this.btnMuteMic.Checked)
					this.btnMuteMic.PerformClick();

				this.btnMuteMic.Enabled = false;

				gablarski.CurrentUser.MutePlayback();
			}
			else
			{
				gablarski.CurrentUser.UnmutePlayback();

				this.btnMuteMic.Enabled = true;
			}
		}

		private void btnMuteMic_Click (object sender, EventArgs e)
		{
			if (gablarski == null)
				return;

			if (this.btnMuteMic.Checked)
				gablarski.CurrentUser.MuteCapture();
			else
				gablarski.CurrentUser.UnmuteCapture();
		}

		private void btnComment_Click (object sender, EventArgs e)
		{
			var changeComment = new CommentForm (this.gablarski.CurrentUser.Comment);
			if (changeComment.ShowDialog() == DialogResult.OK)
				this.gablarski.CurrentUser.SetComment (changeComment.Comment);
		}
	}
}