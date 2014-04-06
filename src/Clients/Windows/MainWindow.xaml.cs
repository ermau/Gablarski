// Copyright (c) 2013, Eric Maupin
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

using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Gablarski.Clients.Messages;
using Gablarski.Clients.Persistence;
using Gablarski.Clients.ViewModels;
using Tempest;
using Tempest.Social;

namespace Gablarski.Clients.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = new MainWindowViewModel (Program.SocialClient, Program.Key.Result);

			Messenger.Register<AddBuddyMessage> (OnAddBuddyMessage);
		}

		private void OnClickAcceptName (object sender, RoutedEventArgs e)
		{
			
		}

		private void OnMouseUpNickname (object sender, MouseButtonEventArgs e)
		{
			
		}

		private void OnDoubleClickBuddy (object sender, MouseButtonEventArgs e)
		{
			
		}

		private void OnAddBuddyMessage (AddBuddyMessage addBuddyMessage)
		{
			new AddBuddyWindow {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			}.Show();
		}

		private void OnServerDoubleClick (object sender, MouseButtonEventArgs e)
		{
			var server = this.servers.SelectedItem as ServerEntry;
			if (server == null)
				return;

		}

		private async void OnStartLocalServer (object sender, RoutedEventArgs e)
		{
			await LocalServer.StartAsync (Program.Key.Result);

		}

		private void OnClickSettings (object sender, RoutedEventArgs e)
		{
			new SettingsWindow {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			}.ShowDialog();
		}
	}
}
