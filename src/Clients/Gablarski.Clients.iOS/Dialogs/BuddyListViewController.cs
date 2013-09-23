using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Gablarski.Clients.ViewModels;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using Tempest.Social;

namespace Gablarski.Clients.iOS.Dialogs
{
	class BuddyListRootController
		: UINavigationController
	{
		public BuddyListRootController()
			: base (new BuddyListViewController())
		{
			TabBarItem = new UITabBarItem (UITabBarSystemItem.Contacts, 0);
		}
	}

	class BuddyListViewController
		: DialogViewController
	{
		public BuddyListViewController()
			: base (UITableViewStyle.Grouped, null, false)
		{
			NavigationItem.RightBarButtonItem = new UIBarButtonItem (UIBarButtonSystemItem.Add, OnAddBuddy);

			Root = new RootElement ("Buddies") {
				new Section()
			};
		}

		private void OnAddBuddy (object sender, EventArgs eventArgs)
		{
			NavigationController.PushViewController (new SearchBuddyViewController(), true);
		}

		public override async void ViewDidLoad()
		{
			base.ViewDidLoad();

			await AppDelegate.Setup;
			viewModel = new BuddyListViewModel (AppDelegate.SocialClient);
			viewModel.PropertyChanged += OnViewModelPropertyChanged;

			LoadBuddies();
		}

		private void OnViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case "Buddies":
					SetupBuddies();
					break;
			}
		}

		private void SetupBuddies()
		{
			var incc = viewModel.Buddies as INotifyCollectionChanged;
			if (incc != null)
				incc.CollectionChanged += OnBuddiesChanged;
		}

		private void OnBuddiesChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			LoadBuddies();
		}

		private void LoadBuddies()
		{
			InvokeOnMainThread (() => {
				Root.First().Clear();

				lock (this.viewModel.Buddies) {
					var section = Root.First();
					section.AddAll (this.viewModel.Buddies.Select (p => {
						var element = new StyledStringElement (p.Nickname);
						if (p.Status != Status.Online)
							element.Font = UIFont.ItalicSystemFontOfSize (element.Font.PointSize);

						return element;
					}));
				}
			});
		}

		private BuddyListViewModel viewModel;
	}
}
