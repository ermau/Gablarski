// Copyright (c) 2010, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gablarski.Client;
using Gablarski.Clients.Windows.Properties;

namespace Gablarski.Clients.Windows
{
	public partial class RegisterForm
		: Form
	{
		private readonly GablarskiClient client;

		public RegisterForm (GablarskiClient client)
		{
			if (client == null)
				throw new ArgumentNullException ("client");

			this.client = client;
			this.client.Disconnected += OnDisconnected;
			Icon = Resources.UserAddImage.ToIcon();
			InitializeComponent();
		}

		public string Username
		{
			get { return this.username.Text; }
		}

		public string Password
		{
			get { return this.password.Text; }
		}

		private void okButton_Click (object sender, EventArgs e)
		{
			if (this.password2.Text != this.password.Text)
				this.password2.BackColor = Color.Red;
			else
			{
				this.okButton.Enabled = false;
				this.loading.Visible = true;
				this.client.CurrentUser.ReceivedRegisterResult += OnReceivedRegisterResult;
				this.client.CurrentUser.Register (this.username.Text, this.password.Text);
			}
		}

		private void OnReceivedRegisterResult (object sender, ReceivedRegisterResultEventArgs e)
		{
			Invoke ((Action)(() =>
			{
				this.client.CurrentUser.ReceivedRegisterResult -= OnReceivedRegisterResult;

				this.loading.Visible = false;
				this.okButton.Enabled = true;

				if (e.Result == RegisterResult.Success)
				{
					DialogResult = DialogResult.Abort;
					Close();
				}
				else
				{
					switch (e.Result)
					{
						case RegisterResult.FailedPassword:
							this.password.BackColor = Color.Red;
							new EditBalloon (this.password) { Title = "Registration Error", Text = "The entered password was invalid.", Icon = TooltipIcon.Warning }.Show();
							break;

						case RegisterResult.FailedNotApproved: // TODO Probably shouldn't let them get here.
							Close();
							MessageBox.Show ("The administrator has not approved you for registration", "Registration", MessageBoxButtons.OK,
							                 MessageBoxIcon.Exclamation);
							break;

						case RegisterResult.FailedUsername:
							this.username.BackColor = Color.Red;
							new EditBalloon (this.username) { Title = "Registration Error", Text = "The entered username was invalid.", Icon = TooltipIcon.Warning }.Show();
							break;

						case RegisterResult.FailedUsernameInUse:
							this.username.BackColor = Color.Red;
							new EditBalloon (this.username) { Title = "Registration Error", Text = "The entered username is already in use.", Icon = TooltipIcon.Warning }.Show();
							break;

						case RegisterResult.FailedUnknown:
						case RegisterResult.FailedUnsupported:
							DialogResult = DialogResult.Abort;
							Close();
							MessageBox.Show ("Either the server does not support registration, or something went really terribly wrong.",
							                 "Registration", MessageBoxButtons.OK,
							                 MessageBoxIcon.Error);
							break;
					}
				}
			}));
		}

		private void OnDisconnected (object sender, DisconnectedEventArgs e)
		{
			Invoke ((Action)Close);
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			this.client.CurrentUser.ReceivedRegisterResult -= OnReceivedRegisterResult;

			DialogResult = DialogResult.Cancel;
			Close();
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			this.client.Disconnected -= OnDisconnected;

			base.OnClosing (e);
		}
	}
}
