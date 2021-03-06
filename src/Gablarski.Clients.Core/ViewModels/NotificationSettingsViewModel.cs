﻿// Copyright (c) 2013, Eric Maupin
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
using Cadenza.Collections;

namespace Gablarski.Clients.ViewModels
{
	public class NotificationSettingsViewModel
		: ViewModelBase
	{
		public NotificationSettingsViewModel()
		{
			Notifiers = Modules.Notifiers.Concat (Modules.TextToSpeech.Cast<INamedComponent>())
						.Select (n => new NotifierViewModel (n, Settings.EnabledNotifications.Contains (n.GetType().GetSimpleName())))
						.ToArray();

			NotificationsEnabled = Settings.EnableNotifications;

			this.globalNotifications = new MutableLookup<string, NotificationTypeViewModel> ();
			foreach (string notifier in Notifiers.Select (n => n.Notifier.GetType().GetSimpleName())) {
				this.globalNotifications.Add (notifier,
					Enum.GetValues (typeof (NotificationType))
						.Cast<NotificationType>()
						.Select (t =>
							new NotificationTypeViewModel (t, Settings.EnabledNotifications[notifier].Contains (t))));
			}

			CurrentNotifier = Notifiers.FirstOrDefault();
		}

		private bool notificationsEnabled;
		public bool NotificationsEnabled
		{
			get { return this.notificationsEnabled; }
			private set
			{
				if (this.notificationsEnabled == value)
					return;

				this.notificationsEnabled = value;
				OnPropertyChanged();
			}
		}

		public IEnumerable<NotifierViewModel> Notifiers
		{
			get;
			private set;
		}

		private NotifierViewModel currentNotifier;
		public NotifierViewModel CurrentNotifier
		{
			get { return this.currentNotifier; }
			set
			{
				if (this.currentNotifier == value)
					return;

				this.currentNotifier = value;
				OnPropertyChanged();
				OnPropertyChanged ("EnabledNotifications");
			}
		}

		private readonly MutableLookup<string, NotificationTypeViewModel> globalNotifications;

		public IEnumerable<NotificationTypeViewModel> EnabledNotifications
		{
			get
			{
				if (CurrentNotifier == null)
					return null;

				return this.globalNotifications[CurrentNotifier.Notifier.GetType().GetSimpleName()];
			}
		}

		public void UpdateSettings()
		{
			Settings.EnableNotifications = NotificationsEnabled;
			
			var notifications = new MutableLookup<string, NotificationType>();
			foreach (var notifier in Notifiers.Where (vm => vm.IsEnabled))
			{
				string name = notifier.Notifier.GetType().GetSimpleName();
				notifications.Add (name, this.globalNotifications[name].Where (vm => vm.IsEnabled).Select (vm => vm.Type));
			}

			Settings.EnabledNotifications = notifications;
		}
	}
}
