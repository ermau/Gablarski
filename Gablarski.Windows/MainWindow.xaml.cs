using System;
using System.Windows;
using System.Windows.Data;
using Gablarski.ViewModels;
using Tempest.Social;

namespace Gablarski.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			
			var vm = new MainWindowViewModel (App.Client);
			vm.ConnectionRequested += OnConnectionRequested ;
			BindingOperations.EnableCollectionSynchronization (vm.BuddyList.Buddies, new object());
			DataContext = vm;

			CollectionViewSource viewSource = (CollectionViewSource)TryFindResource ("BuddiesByStatus");
			viewSource.Filter += (sender, args) =>
			{
				Person p = (Person)args.Item;
				args.Accepted = p.Status != Status.Offline;
			};
		}

		private void OnConnectionRequested (object sender, RequestConnectEventArgs e)
		{
			MessageBoxResult result = MessageBox.Show (e.Person.Nickname + " is calling, accept?", "Incoming call", MessageBoxButton.OKCancel);
			if (result == MessageBoxResult.OK)
				e.Accept = true;
		}

		private void OnClickAcceptName (object sender, RoutedEventArgs e)
		{
			this.nickname.BindingGroup.UpdateSources();
		}
	}
}
