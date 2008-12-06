using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GablarskiClient
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow ()
		{
			InitializeComponent ();
		}

		private Gablarski.Client.GablarskiClient client;

		private void connectButton_Click(object sender, RoutedEventArgs e)
		{
			client = new Gablarski.Client.GablarskiClient();
			client.Connected	+= client_Connected;
			client.Disconnected += client_Disconnected;
			client.LoggedIn		+= client_LoggedIn;
			client.UserLogin	+= client_UserLogin;
			client.UserLogout	+= client_UserLogout;
			client.Connect (this.host.Text, 6112);
			client.Login (this.nickname.Text);
		}

		void client_UserLogout (object sender, Gablarski.Server.UserEventArgs e)
		{
			this.Log (e.User.Username + " has disconnected.");
		}

		void client_UserLogin (object sender, Gablarski.Server.UserEventArgs e)
		{
			this.Log (e.User.Username + " has connected.");
		}

		void client_LoggedIn (object sender, Gablarski.ConnectionEventArgs e)
		{
			this.SetStatusImage ("./Resources/key.png");
			this.Log ("Logged in.");
		}

		private void client_Disconnected (object sender, Gablarski.ReasonEventArgs e)
		{
			this.SetStatusImage ("./Resources/disconnect.png");
			this.Log ("Disconnected: " + e.Reason);
		}

		private void client_Connected (object sender, Gablarski.ConnectionEventArgs e)
		{
			this.SetStatusImage ("./Resources/connect.png");
			this.Log ("Connected.");
		}

		private void SetStatusImage (string uri)
		{
			this.Dispatcher.BeginInvoke((Action)delegate
			{
				this.statusImage.Source =
					new BitmapImage(new Uri(uri, UriKind.Relative));
			});
		}

		private void Log (string log)
		{
			this.Dispatcher.BeginInvoke ((Action)delegate
			{
				this.log.Text += log + Environment.NewLine;
			});
		}

		private void Window_Closed (object sender, EventArgs e)
		{
			this.client.Disconnect ();
		}
	}
}