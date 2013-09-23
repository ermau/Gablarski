using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace Gablarski.Clients.iOS.Dialogs
{
	class SettingsRootDialogController
		: UINavigationController
	{
		public SettingsRootDialogController()
			: base (new SettingsDialogController())
		{
			TabBarItem = new UITabBarItem ("Settings", null, 0);
		}
	}

	class SettingsDialogController
		: DialogViewController
	{
		public SettingsDialogController()
			: base (UITableViewStyle.Grouped, null, false)
		{
			NavigationItem.RightBarButtonItem = new UIBarButtonItem ("Save", UIBarButtonItemStyle.Plain, OnSave);

			Root = new RootElement("Settings") {
				new Section ("Persona") {
					this.nickname
				}
			};
		}

		private readonly EntryElement nickname = new EntryElement (null, "Nickname", Settings.Nickname);

		private async void OnSave (object sender, EventArgs eventArgs)
		{
			this.nickname.FetchValue();
			Settings.Nickname = this.nickname.Value;

			await Settings.SaveAsync();
		}
	}
}
