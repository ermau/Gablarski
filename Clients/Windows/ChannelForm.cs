using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gablarski.Clients.Windows.Properties;

namespace Gablarski.Clients.Windows
{
	public partial class ChannelForm
		: Form
	{
		public ChannelForm ()
		{
			InitializeComponent();

			this.Channel = new Channel ();
			this.Text = "Add Channel";
			this.Icon = Resources.ChannelAddImage.ToIcon();
		}

		public ChannelForm (Channel channel)
		{
			InitializeComponent ();

			this.Channel = new Channel (channel.ChannelId, channel);
			this.Text = "Edit Channel";
			this.Icon = Resources.ChannelEditImage.ToIcon();

			this.inName.Text = channel.Name;
			this.inDescription.Text = channel.Description;
			this.inPlayerLimit.Value = channel.PlayerLimit;
		}

		public Channel Channel
		{
			get; private set;
		}

		private void btnOk_Click (object sender, EventArgs e)
		{
			this.ValidateChildren (ValidationConstraints.Visible);

			this.Channel.Name = this.inName.Text.Trim();
			this.Channel.Description = this.inDescription.Text.Trim();
			this.Channel.PlayerLimit = (int)this.inPlayerLimit.Value;

			this.Close();
		}
	}
}