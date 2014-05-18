//
// MainWindow.xaml.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2013-2014 Xamarin Inc.
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
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Gablarski.Clients.Messages;
using Gablarski.Clients.Persistence;
using Gablarski.Clients.ViewModels;

namespace Gablarski.Clients.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		//private static readonly ChatViewModel ChatViewModel = new ChatViewModel (Program.SocialClient, Program.History);

		public MainWindow()
		{
			InitializeComponent();

			IntPtr windowHandle = new WindowInteropHelper (this).EnsureHandle();

			DataContext = new MainWindowViewModel (Program.Key.Result, windowHandle);

			/*Messenger.Register<AddBuddyMessage> (OnAddBuddyMessage);
			Messenger.Register<StartChatMessage> (OnStartChatMessage);*/
		}

		private void OnClickAcceptName (object sender, RoutedEventArgs e)
		{
			
		}

		private void OnMouseUpNickname (object sender, MouseButtonEventArgs e)
		{
			
		}

		/*
		private void OnStartChatMessage (StartChatMessage startChatMessage)
		{
			ChatViewModel chatVM = ChatViewModel;

			var history = Program.History.GetMessages (startChatMessage.Group);
			BindingOperations.EnableCollectionSynchronization (history, history);

			var window = new ChatWindow {
				DataContext = chatVM,
				WindowStartupLocation = WindowStartupLocation.CenterScreen
			};

			window.Show();
		}

		private void OnAddBuddyMessage (AddBuddyMessage addBuddyMessage)
		{
			new AddBuddyWindow {
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			}.Show();
		}*/

		private void OnServerDoubleClick (object sender, MouseButtonEventArgs e)
		{
			var servervm = (ServerEntryViewModel) this.servers.SelectedItem;
			if (servervm == null)
				return;

			Messenger.Send (new JoinServerMessage (servervm.Server));
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

		private void OnClickAddServer (object sender, RoutedEventArgs e)
		{
			Messenger.Send (new EditServerMessage (new ServerEntry (0)));
		}

		private void OnClickEditServer (object sender, RoutedEventArgs e)
		{
			if (this.servers.SelectedItem == null)
				return;

			ServerEntryViewModel entry = this.servers.SelectedItem as ServerEntryViewModel;
			if (entry == null)
				return;

			Messenger.Send (new EditServerMessage (entry.Server));
		}
	}
}
