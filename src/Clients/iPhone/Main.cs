
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreFoundation;
using Gablarski;
using Gablarski.Client;
using Gablarski.Network;
using Mono.Rocks;
using System.Net;
using MonoTouch.ObjCRuntime;
using System.Reflection;

namespace Gablarski.Clients.iPhone
{
	public class Application
	{
		static void Main (string[] args)
		{
			UIApplication.Main (args);
		}
	}

	// The name AppDelegate is referenced in the MainWindow.xib file.
	public partial class AppDelegate : UIApplicationDelegate
	{
		// This method is invoked when the application has loaded its UI and its ready to run
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// If you have defined a view, add it here:
			window.AddSubview (tabs.View);
			window.MakeKeyAndVisible ();
			
			return true;
		}
		
		private class AutoReleasePool
		{
			public AutoReleasePool()
			{
				handle = Messaging.intptr_objc_msgSend (new Class ("NSAutoreleasePool").Handle, new Selector ("alloc").Handle);
				handle = Messaging.intptr_objc_msgSend (handle, new Selector ("init").Handle);
			}
			
			public void Release()
			{
				Messaging.void_objc_msgSend (handle, new Selector ("release").Handle);
			}			
			
			private IntPtr handle;
		}
		
		private GablarskiClient client;
		partial void Connect (MonoTouch.UIKit.UIButton sender)
		{
			client = new GablarskiClient (new NetworkClientConnection { VerboseTracing = true }) { VerboseTracing = true };
			client.Connected += (s, e) =>
			{
				var pool = new AutoReleasePool();
				
				this.client.CurrentUser.Login (this.nicknameIn.Text);
				
				pool.Release();
			};
			
			client.ConnectionRejected += (object s, RejectedConnectionEventArgs e) =>
			{
				var pool = new AutoReleasePool();
				
				Alert ("Connect failed", e.Reason.ToString() + ((e.Error != null) ? e.Error.ToString() : String.Empty)).Dispose();
				
				pool.Release();
			};
			
			client.Disconnected += (s, e) =>
			{
				var pool = new AutoReleasePool();
				
				Alert ("Disconnected", "Disconnected").Dispose();
				
				pool.Release();
			};			
			
			client.CurrentUser.ReceivedLoginResult += (object s, ReceivedLoginResultEventArgs e) => 
			{
				Console.WriteLine ("Login Result: " + e.Result.ResultState);
			};
			
			client.Connect (this.serverIn.Text, 6112);
		}

		private UIAlertView Alert (string header, string message)
		{
			UIAlertView alert = new UIAlertView (header, message, null, "Cancel", "OK");
			alert.Show();
			return alert;
		}

		// This method is required in iPhoneOS 3.0
		public override void OnActivated (UIApplication application)
		{
		}
	}
}