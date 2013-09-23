using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gablarski.Clients.iOS.Dialogs;
using Gablarski.Clients.Persistence;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Tempest;
using Tempest.Social;

namespace Gablarski.Clients.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		public static GablarskiSocialClient SocialClient;
		public static Task Setup;

		// class-level declarations
		UIWindow window;

		private Task<RSAAsymmetricKey> key;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			ClientData.Setup (useLocalFiles: true);
			this.key = ClientData.GetCryptoKeyAsync();

			UIViewController rootViewController;
			if (Settings.Nickname == null)
				rootViewController = new UINavigationController (new SetupDialogController());
			else {
				StartSetup();
				rootViewController = new MainViewController();
			}

			window = new UIWindow (UIScreen.MainScreen.Bounds);
			window.RootViewController = rootViewController;
			window.MakeKeyAndVisible();

			return true;
		}

		public static void StartSetup()
		{
			Setup = SetupAsync();
		}

		static async Task SetupAsync()
		{
			var app  = (AppDelegate)UIApplication.SharedApplication.Delegate;

			RSAAsymmetricKey key = await app.key.ConfigureAwait (false);

			string id = key.PublicSignature.Aggregate (String.Empty, (s, b) => s + b.ToString ("X2"));
			var person = new Person (id) {
				Nickname = Settings.Nickname,
				Avatar = Settings.Avatar,
				Status = Status.Online
			};

			SocialClient = new GablarskiSocialClient (person, key);
			SocialClient.SetTarget (new Target ("192.168.1.6", SocialProtocol.DefaultPort));
		}
	}
}