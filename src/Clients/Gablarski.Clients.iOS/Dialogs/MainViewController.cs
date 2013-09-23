using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoTouch.UIKit;

namespace Gablarski.Clients.iOS.Dialogs
{
	class MainViewController
		: UITabBarController
	{
		public MainViewController()
		{
			var controllers = new UIViewController[] {
				new BuddyListRootController(),
				new SettingsRootDialogController(),
			};

			SetViewControllers (controllers, animated: false);
		}
	}
}
