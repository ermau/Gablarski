using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Gablarski.Client;
using Gablarski.Clients;
using Gablarski.Clients.Windows.Entities;
using Gablarski.Clients.Windows.Properties;
using Gablarski.Messages;
using Gablarski.Server;
using Cadenza;

namespace Gablarski.Clients.Windows
{
	public partial class LoginForm
		: Form
	{
		private readonly static Image serverOkImage;
		private static readonly Image serverQueryImage;
		private static readonly Image serverConnectErrorImage;
		private static readonly Image serverBannedImage;
		private static readonly Image serverVersionErrorImage;

		static LoginForm()
		{
			serverOkImage = Resources.LServerImage.Overlay (Resources.OKOverlay, ContentAlignment.BottomRight);
			serverQueryImage = Resources.LServerImage.Overlay (Resources.HourglassImage, new Size (16, 16), ContentAlignment.BottomRight);
			serverConnectErrorImage = Resources.LServerImage.Overlay (Resources.HaltOverlay, ContentAlignment.BottomRight);
			serverBannedImage = Resources.LServerImage.Overlay (Resources.BanImage, new Size (16, 16), ContentAlignment.BottomRight);
			serverVersionErrorImage = Resources.LServerImage.Overlay (Resources.ErrorOverlay, ContentAlignment.BottomRight);
		}

		public LoginForm ()
		{
			InitializeComponent();

			this.Icon = Resources.ServerConnectImage.ToIcon();
		}

		public ServerEntry Entry
		{
			get; private set;
		}

		private HashSet<IPEndPoint> localServers = new HashSet<IPEndPoint>();
		private void DisplayLocalServer (IEnumerable<Cadenza.Tuple<ServerInfo, IPEndPoint>> foundServers)
		{
			if (this.Disposing || this.IsDisposed)
				return;

			if (this.InvokeRequired)
			{
			    BeginInvoke ((Action<IEnumerable<Cadenza.Tuple<ServerInfo, IPEndPoint>>>)DisplayLocalServer, foundServers);
			    return;
			}

			var group = this.servers.Groups.Cast<ListViewGroup>().First (g => g.Name == "local");

			servers.BeginUpdate();

			var found = foundServers.ToDictionary (t => t.Item2);
			var updated = localServers.Intersect (found.Keys).Select (e => found[e]);
			var deleted = localServers.Where (s => !found.ContainsKey (s));
			var newly = foundServers.Where (s => !localServers.Contains (s.Item2));

			foreach (var server in updated.Concat (newly))
			{
				var info = server.Item1;
				var endpoint = server.Item2;

				string key = endpoint.Address + ":" + endpoint.Port;

				ListViewItem li;
				if (servers.Items.ContainsKey (key))
				{
					li = servers.Items[key];
					li.Text = server.Item1.Name;
				}
				else
					li = servers.Items.Add (key, info.Name, 0);

				li.Tag = new ServerEntry (0) { Name = info.Name, Host = endpoint.Address.ToString(), Port = endpoint.Port };
				group.Items.Add (li);
			}

			foreach (var endpoint in deleted)
			{
				string key = endpoint.Address + ":" + endpoint.Port;

				if (servers.Items.ContainsKey (key))
					servers.Items.RemoveByKey (key);
			}
			servers.EndUpdate();

			this.localServers = new HashSet<IPEndPoint> (foundServers.Select (s => s.Item2));
		}

		private void servers_SelectedIndexChanged (object sender, EventArgs e)
		{
			switch (this.servers.SelectedItems.Count)
			{
				case 0:
					this.ShowAddServer();
					this.btnConnect.Enabled = false;
					this.Entry = null;
					break;

				case 1:
					this.ShowEditServer();
					this.btnConnect.Enabled = true;
					this.Entry = this.servers.SelectedItems[0].Tag as ServerEntry;
					break;

				default:
					this.btnConnect.Enabled = false;
					this.btnAddServer.Enabled = false;
					this.Entry = null;
					break;
			}
		}

		private void servers_ItemActivate (object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void ShowAddServer()
		{
			this.btnEditServer.Visible = false;
			this.btnAddServer.Visible = true;
			this.btnSaveServer.Visible = false;
		}

		private void ShowEditServer()
		{
			this.btnEditServer.Visible = true;
			this.btnAddServer.Visible = false;
			this.btnSaveServer.Visible = false;
		}

		private void EditServer()
		{
			this.ShowSaveServer();
			this.inName.Text = this.Entry.Name;
			this.inServer.Text = this.Entry.Host;
			this.inPort.Text = this.Entry.Port.ToString();
			this.inServerPassword.Text = this.Entry.ServerPassword;
			this.inNickname.Text = this.Entry.UserNickname;
			this.inPhonetic.Text = this.Entry.UserPhonetic;
			this.inUsername.Text = this.Entry.UserName;
			this.inPassword.Text = this.Entry.UserPassword;
			this.pnlModServer.Visible = true;

			this.btnConnect.Enabled = false;
		}

		private void ClearEdit()
		{
			this.serverStatus.Image = null;
			this.pnlModServer.Visible = false;

			this.inName.Clear();
			this.inName.BackColor = Color.Empty;

			this.inServer.Clear();
			this.inServer.BackColor = Color.Empty;

			this.inPort.Text = "42912";
			this.inServerPassword.Clear();
			
			this.inNickname.Clear();
			this.inNickname.BackColor = Color.Empty;

			this.inUsername.Clear();
			this.inPassword.Clear();

			this.ShowAddServer();
		}

		private void ShowSaveServer()
		{
			this.btnEditServer.Visible = false;
			this.btnAddServer.Visible = false;
			this.btnSaveServer.Visible = true;
		}

		private void btnAddServer_Click (object sender, EventArgs e)
		{
			this.pnlModServer.Visible = true;
			this.ShowSaveServer();
		}

		private void btnEditServer_Click (object sender, EventArgs e)
		{
			this.EditServer();
		}

		private void btnSaveServer_Click (object sender, EventArgs e)
		{
			if (!ValidateChildren (ValidationConstraints.Visible))
				return;

			if (this.Entry == null)
				this.Entry = new ServerEntry(0);

			this.Entry.Name = this.inName.Text.Trim();
			this.Entry.Host = this.inServer.Text.Trim();
			this.Entry.Port = Int32.Parse (this.inPort.Text.Trim());
			this.Entry.ServerPassword = this.inServerPassword.Text.Trim();
			this.Entry.UserNickname = this.inNickname.Text.Trim();
			this.Entry.UserPhonetic = this.inPhonetic.Text.Trim();
			this.Entry.UserName = this.inUsername.Text.Trim();
			this.Entry.UserPassword = this.inPassword.Text.Trim();
			Servers.SaveServer (Entry);
			Program.UpdateTaskbarServers();

			this.ClearEdit();
			this.LoadServerEntries();
		}

		private void btnCancel_Click (object sender, EventArgs e)
		{
			if (this.pnlModServer.Visible)
			{
				this.ClearEdit ();
				if (this.servers.SelectedItems.Count > 0)
					this.btnConnect.Enabled = true;
			}
			else
				this.Close ();
		}

		private void LoadServerEntries()
		{
			this.servers.BeginUpdate();
			this.servers.Items.Clear();

			GablarskiClient.FindLocalServers (0, DisplayLocalServer, () => !(this.IsDisposed || !this.Visible));

			this.servers.Groups.Add ("local", "Local Servers");
			var saved = this.servers.Groups.Add ("dbentries", "Saved Servers");

			foreach (var entry in Servers.GetEntries())
			{
				var li = this.servers.Items.Add (entry.Name);
				li.Tag = entry;
				li.ImageKey = "serverQuery";
				saved.Items.Add (li);

				GablarskiClient.QueryServer (entry.Host, entry.Port, li, ServerQueried);
			}

			this.servers.EndUpdate();
		}

		private void UpdateImage (ListViewItem li, string key)
		{
			if (IsDisposed || Disposing)
				return;

			li.ImageKey = key;
		}

		private void ServerQueried (ServerQuery query)
		{
			if (IsDisposed || Disposing || this.closing)
				return;

			var li = (ListViewItem)query.Tag;

			switch (query.RejectedReason)
			{
				case ConnectionRejectedReason.CouldNotConnect:
					BeginInvoke((Action<ListViewItem, string>)UpdateImage, li, "serverConnectError");
					return;

				case ConnectionRejectedReason.BanHammer:
					BeginInvoke((Action<ListViewItem, string>)UpdateImage, li, "serverBanned");
					return;

				case ConnectionRejectedReason.IncompatibleVersion:
					BeginInvoke((Action<ListViewItem, string>)UpdateImage, li, "serverVersionError");
					return;

				case ConnectionRejectedReason.Unknown:
				{
					BeginInvoke ((Action<ServerQuery>)(q =>
					{
						li.ToolTipText = String.Format ("{0}{1}Users: {2}", q.ServerInfo.Description, Environment.NewLine, q.Users.Count());
					}), query);

					BeginInvoke ((Action<ListViewItem, string>)UpdateImage, li, "server");

					if (!String.IsNullOrEmpty (query.ServerInfo.Logo))
					{
						Uri logoUri;
						if (Uri.TryCreate (query.ServerInfo.Logo, UriKind.Absolute, out logoUri))
						{
							try
							{
								WebClient client = new WebClient();
								byte[] data = client.DownloadData (query.ServerInfo.Logo);

								Image logo = Image.FromStream (new MemoryStream (data));
								BeginInvoke ((Action<Image>)(l =>
								{
									if (IsDisposed || Disposing)
										return;

									int index = li.ImageList.Images.Add (l, Color.Transparent);
									li.ImageIndex = index;
								}), logo);
							}
							catch
							{
							}
						}
					}

					break;
				}
			}
		}

		private void LoginForm_Load (object sender, EventArgs e)
		{
			ImageList largeImageList = new ImageList();
			largeImageList.ColorDepth = ColorDepth.Depth24Bit;
			largeImageList.TransparentColor = Color.Transparent;
			largeImageList.ImageSize = new Size (24, 24);
			largeImageList.Images.Add ("server", serverOkImage);
			largeImageList.Images.Add ("serverQuery", serverQueryImage);
			largeImageList.Images.Add ("serverConnectError", serverConnectErrorImage);
			largeImageList.Images.Add ("serverBanned", serverBannedImage);
			largeImageList.Images.Add ("serverVersionError", serverVersionErrorImage);

			this.servers.LargeImageList = largeImageList;

			Settings.SettingChanged += OnSettingsChanged;
			SetupTTS();

			this.LoadServerEntries();
		}

		private void LoginForm_FormClosing (object sender, FormClosingEventArgs e)
		{
			Settings.SettingChanged -= OnSettingsChanged;
			this.closing = true;
		}

		private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != Settings.TextToSpeechSettingName)
				return;

			SetupTTS();
		}

		private void SetupTTS ()
		{
			playPhonetic.Enabled = Modules.TextToSpeech.Any (tts => tts.GetType().GetSimpleName() == Settings.TextToSpeech);
		}

		private void startLocal_Click (object sender, EventArgs e)
		{
			InputForm nickname = new InputForm();
			if (nickname.ShowDialogOnFormThread (this) == DialogResult.Cancel)
				return;

			string nick = nickname.Input.Text.Trim();

			LocalServer.Start();

			this.DialogResult = DialogResult.OK;
			this.Entry = new ServerEntry (0)
			{
				Host = "localhost",
				Port = 42912,
				UserName = nick,
				UserNickname = nick,
				UserPassword = "password"
			};
		}

		private void settingsButton_Click (object sender, EventArgs e)
		{
			SettingsForm sf = new SettingsForm();
			sf.ShowDialog();
		}

		private void servers_KeyUp (object sender, KeyEventArgs e)
		{
			if (e.Shift && e.Control && e.Alt && e.KeyCode == Keys.Delete)
				throw new InvalidOperationException ("Error reporting test");
			if (e.KeyCode == Keys.Delete && this.servers.SelectedItems.Count != 0)
				DeleteServers ();
		}

		private void DeleteServers ()
		{
			foreach (var li in this.servers.SelectedItems.Cast<ListViewItem> ())
			{
				var s = (ServerEntry)li.Tag;
				Servers.DeleteServer (s);
				this.servers.Items.Remove (li);
			}

			Program.UpdateTaskbarServers();
		}

		private void playPhonetic_Click (object sender, EventArgs e)
		{
			ITextToSpeech tts = Modules.TextToSpeech.FirstOrDefault (t => t.GetType().GetSimpleName() == Settings.TextToSpeech);
			if (tts != null)
				tts.Say (this.inPhonetic.Text);
		}

		private bool lastWasPort = false;
		private readonly object addServerQuerySync = new object();
		private bool closing;

		private void AddServerQuery (ServerQuery query)
		{
			bool port = (bool)query.Tag;
			lock (addServerQuerySync)
			{
				bool last = lastWasPort;
				lastWasPort = port;
				if (last && !port)
					return;

				switch (query.RejectedReason)
				{
					case ConnectionRejectedReason.BanHammer:
						this.serverStatus.Image = serverBannedImage;
						break;

					case ConnectionRejectedReason.CouldNotConnect:
						this.serverStatus.Image = serverConnectErrorImage;
						break;

					case ConnectionRejectedReason.IncompatibleVersion:
						this.serverStatus.Image = serverVersionErrorImage;
						break;

					case ConnectionRejectedReason.Unknown:
						if (query.ServerInfo != null)
						{
							BeginInvoke ((Action<ServerInfo>)(serverInfo =>
							{
								if (this.inName.Text == String.Empty)
									this.inName.Text = serverInfo.Name;

								this.serverStatus.Image = serverOkImage;
							}), query.ServerInfo);
						}

						break;
				}
			}
		}

		private void inServer_Validated (object sender, EventArgs e)
		{
			lock (addServerQuerySync)
				lastWasPort = false;

			int port = (int)this.inPort.Value;

			if (this.inServer.Text.IsNullOrWhitespace())
				return;
			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				return;

			this.serverStatus.Image = serverQueryImage;
			GablarskiClient.QueryServer (this.inServer.Text, port, false, AddServerQuery);
		}

		private void inPort_Validated (object sender, EventArgs e)
		{
			int port = (int)this.inPort.Value;

			if (this.inServer.Text.IsNullOrWhitespace())
				return;
			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				return;

			this.serverStatus.Image = serverQueryImage;
			GablarskiClient.QueryServer (this.inServer.Text, port, true, AddServerQuery);
		}
	}
}