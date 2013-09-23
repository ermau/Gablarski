using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Gablarski.Clients.ViewModels;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using Tempest.Social;

namespace Gablarski.Clients.iOS.Dialogs
{
	class SearchBuddyViewController
		: DialogViewController
	{
		public SearchBuddyViewController()
			: base (UITableViewStyle.Grouped, null)
		{
			NavigationItem.LeftBarButtonItem = new UIBarButtonItem ("Cancel", UIBarButtonItemStyle.Plain,
				(o, e) => DismissViewController (true, null));

			NavigationItem.RightBarButtonItem = new UIBarButtonItem ("Done", UIBarButtonItemStyle.Done, OnAddBuddies);

			this.search.Changed += SearchOnChanged;
			this.viewModel.PropertyChanged += OnViewModelPropertyChanged;
			
			Root = new RootElement("Add Buddy") {
				new Section {
					this.search
				},

				this.resultsSection
			};
		}

		private void OnAddBuddies (object sender, EventArgs eventArgs)
		{
			foreach (var person in this.selected)
				this.viewModel.AddBuddy.Execute (person);

			DismissViewController (true, null);
		}

		private readonly AddBuddyViewModel viewModel = new AddBuddyViewModel (AppDelegate.SocialClient);

		private readonly Section resultsSection = new Section();
		private readonly EntryElement search = new EntryElement (null, "nickname", null);

		private readonly List<Person> selected = new List<Person>();

		private void SearchOnChanged (object sender, EventArgs eventArgs)
		{
			this.resultsSection.Clear();
			this.resultsSection.Add (new ActivityElement());
			this.viewModel.Search = this.search.Value;
		}

		private void OnViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case "SearchResults":
					INotifyPropertyChanged inpc = this.viewModel.SearchResults;
					if (inpc != null)
						inpc.PropertyChanged += OnSearchResultsPropertyChanged;
					break;
			}
		}

		private void OnSearchResultsPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != "Value")
				return;

			var results = this.viewModel.SearchResults;
			if (results != null)
				return;

			var value = results.Value;
			if (value != null)
				return;

			InvokeOnMainThread (() => {
				this.resultsSection.Clear();
				this.resultsSection.AddAll (value.Select (p => {
					StyledStringElement element = null;
					element = new StyledStringElement (p.Nickname, () => {
						if (element.Accessory == UITableViewCellAccessory.Checkmark) {
							element.Accessory = UITableViewCellAccessory.None;
							this.selected.Remove (p);
						} else {
							element.Accessory = UITableViewCellAccessory.Checkmark;
							this.selected.Add (p);
						}
					});

					if (p.Status != Status.Online)
						element.Font = UIFont.ItalicSystemFontOfSize (element.Font.PointSize);

					return element;
				}));
			});
		}
	}
}
