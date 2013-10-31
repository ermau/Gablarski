using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Gablarski.Clients.Messages;
using Gablarski.Clients.ViewModels;

namespace Gablarski.Clients.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = new MainWindowViewModel (Program.SocialClient);

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
	}
}
