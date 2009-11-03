using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Gablarski.Client;
using Gablarski.Clients;
using Gablarski.Clients.Windows.Entities;
using Gablarski.Clients.Windows.Properties;
using Gablarski.Server;
using Mono.Rocks;

namespace Gablarski.Clients.Windows
{
	public partial class LoginForm
		: Form
	{
		public LoginForm ()
		{
			InitializeComponent ();

			this.Icon = Resources.ServerConnectImage.ToIcon ();
		}

		public ServerEntry Entry
		{
			get; private set;
		}

		private HashSet<IPEndPoint> localServers = new HashSet<IPEndPoint>();
		private void DisplayLocalServer (IEnumerable<Tuple<ServerInfo, IPEndPoint>> foundServers)
		{
			if (this.Disposing || this.IsDisposed)
				return;

			if (this.InvokeRequired)
			{
			    BeginInvoke ((Action<IEnumerable<Tuple<ServerInfo, IPEndPoint>>>)DisplayLocalServer, foundServers);
			    return;
			}

			var group = this.servers.Groups.Cast<ListViewGroup>().First (g => g.Name == "local");

			servers.BeginUpdate();

			var found = foundServers.ToDictionary (t => t._2);
			var updated = localServers.Intersect (found.Keys).Select (e => found[e]);
			var deleted = localServers.Where (s => !found.ContainsKey (s));
			var newly = foundServers.Where (s => !localServers.Contains (s._2));

			foreach (var server in updated.Concat (newly))
			{
				var info = server._1;
				var endpoint = server._2;

				string key = endpoint.Address + ":" + endpoint.Port;

				ListViewItem li;
				if (servers.Items.ContainsKey (key))
				{
					li = servers.Items[key];
					li.Text = server._1.Name;
				}
				else
					li = servers.Items.Add (key, info.Name, 0);

				li.Tag = new ServerEntry { Name = info.Name, Host = endpoint.Address.ToString(), Port = endpoint.Port };
				group.Items.Add (li);
			}

			foreach (var endpoint in deleted)
			{
				string key = endpoint.Address + ":" + endpoint.Port;

				if (servers.Items.ContainsKey (key))
					servers.Items.RemoveByKey (key);
			}
			servers.EndUpdate();

			this.localServers = new HashSet<IPEndPoint> (foundServers.Select (s => s._2));
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
			this.inUsername.Text = this.Entry.UserName;
			this.inPassword.Text = this.Entry.UserPassword;
			this.pnlModServer.Visible = true;

			this.btnConnect.Enabled = false;
		}

		private void ClearEdit()
		{
			this.pnlModServer.Visible = false;

			this.inServer.Clear();
			this.inPort.Clear();
			this.inServerPassword.Clear();
			this.inNickname.Clear();
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
			try
			{
				if (!ValidateChildren (ValidationConstraints.Visible))
					return;

				if (this.Entry == null)
					this.Entry = new ServerEntry();

				this.Entry.Name = this.inName.Text.Trim();
				this.Entry.Host = this.inServer.Text.Trim();
				this.Entry.Port = Int32.Parse (this.inPort.Text.Trim());
				this.Entry.ServerPassword = this.inServerPassword.Text.Trim();
				this.Entry.UserNickname = this.inNickname.Text.Trim();
				this.Entry.UserName = this.inUsername.Text.Trim();
				this.Entry.UserPassword = this.inPassword.Text.Trim();
				Persistance.CurrentSession.SaveOrUpdate (this.Entry);
				Persistance.CurrentSession.Flush();

				this.ClearEdit();
				this.LoadServerEntries();
			}
			catch (Exception)
			{
				throw;
			}
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
				li.ImageIndex = 0;
				saved.Items.Add (li);
			}

			this.servers.EndUpdate();
		}

		private void LoginForm_Load (object sender, EventArgs e)
		{
			ImageList images = new ImageList();
			images.ColorDepth = ColorDepth.Depth24Bit;
			images.TransparentColor = Color.Transparent;
			images.Images.Add (Resources.ServerImage);

			this.servers.LargeImageList = images;
			this.servers.SmallImageList = images;

			this.LoadServerEntries();
		}

		private void startLocal_Click (object sender, EventArgs e)
		{
			InputForm nickname = new InputForm();
			if (nickname.ShowDialogOnFormThread (this) == DialogResult.Cancel)
				return;

			string nick = nickname.Input.Text.Trim();

			LocalServer.Start();

			this.DialogResult = DialogResult.OK;
			this.Entry = new ServerEntry
			{
				Host = "localhost",
				Port = 6112,
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
			if (e.KeyCode != Keys.Delete || this.servers.SelectedItems.Count == 0)
				return;

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

			Persistance.CurrentSession.Flush ();
		}
	}
}