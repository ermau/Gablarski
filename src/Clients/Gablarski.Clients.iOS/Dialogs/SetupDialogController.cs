using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace Gablarski.Clients.iOS.Dialogs
{
	class SetupDialogController
		: DialogViewController
	{
		public SetupDialogController()
			: base (UITableViewStyle.Grouped, null)
		{
			var nickname = new EntryElement (null, "Nickname", null);
			Root = new RootElement("Gablarski") {
				new Section {
					nickname
				},

				new Section {
					new StringElement ("Continue", async () => {
						nickname.FetchValue();
						Settings.Nickname = nickname.Value;
						await Settings.SaveAsync();
						AppDelegate.StartSetup();

						PresentViewController (new MainViewController(), true, null);
					})
				}
			};
		}
	}
}
