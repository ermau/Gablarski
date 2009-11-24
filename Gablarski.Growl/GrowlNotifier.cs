// Copyright (c) 2009, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

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
		
		public void Notify (NotificationType type, string say, string nickname, string phonetic, Clients.NotifyPriority priority)
		{
			Notify (type, String.Format (say, nickname), priority);
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