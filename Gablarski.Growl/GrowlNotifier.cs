using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Growl.Connector;
using System.IO;

namespace Gablarski.Growl
{
	public class GrowlNotifier
		: Clients.INotifier
	{
		public GrowlNotifier ()
		{
			gablarskiApp = new Application ("Gablarski");
			gablarskiApp.Icon = Path.Combine (Environment.CurrentDirectory, "Headphones.ico");

			string[] names = Enum.GetNames (typeof (Gablarski.Clients.NotificationType));
			NotificationType[] types = new NotificationType[names.Length];
			for (int i = 0; i < types.Length; ++i)
			{
				types[i] = new NotificationType (names[i], UpperPascalToNormal (names[i]));
			}

			growl.Register (gablarskiApp, types);
		}

		public string Name
		{
			get { return "Growl"; }
		}

		public Clients.Media.IMediaController Media
		{
			set { }
		}

		public void Notify (Clients.NotificationType type, string contents, Clients.NotifyPriority priority)
		{
			growl.Notify (new Notification (gablarskiApp.Name, type.ToString (), null, UpperPascalToNormal (type.ToString ()), contents)
							{
								Priority = ToPriority (priority)
							});
		}

		private readonly GrowlConnector growl = new GrowlConnector ();
		private readonly Application gablarskiApp;

		private static string UpperPascalToNormal (string s)
		{
			StringBuilder builder = new StringBuilder ();

			int l = 0;
			for (int i = 0; i < s.Length; ++i)
			{
				if (Char.IsUpper (s[i]))
					builder.Append (' ');

				builder.Append (s[i]);
			}

			return builder.ToString ();
		}

		private static Priority ToPriority (Clients.NotifyPriority priority)
		{
			switch (priority)
			{
				case Clients.NotifyPriority.Info:
				default:
					return Priority.Normal;
			}
		}
	}
}