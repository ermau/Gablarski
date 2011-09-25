// Copyright (c) 2011, Eric Maupin
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
using System.ComponentModel;

namespace Gablarski.Server
{
	public class ServerSettings
		: INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private string name = "Gablarski Server";
		public virtual string Name
		{
			get
			{
				lock (settingsLock)
				{
					return this.name;
				}
			}

			set
			{
				lock (settingsLock)
				{
					if (value == this.name)
						return;

					this.name = value;
					OnPropertyChanged ("Name");
				}
			}
		}

		private string description = "Default Gablarski Server";
		public virtual string Description
		{
			get
			{
				lock (settingsLock)
				{
					return this.description;
				}
			}

			set
			{
				lock (settingsLock)
				{
					if (value == this.description)
						return;

					this.description = value;
					OnPropertyChanged ("Description");
				}
			}
		}

		private int minbitrate = 24000;
		public virtual int MinimumAudioBitrate
		{
			get
			{
				lock (settingsLock)
				{
					return this.minbitrate;
				}
			}

			set
			{
				lock (settingsLock)
				{
					if (value == this.minbitrate)
						return;

					this.minbitrate = value;
					OnPropertyChanged ("MinimumAudioBitrate");
				}
			}
		}

		private int maxbitrate = 96000;
		public virtual int MaximumAudioBitrate
		{
			get
			{
				lock (settingsLock)
				{
					return this.maxbitrate;
				}
			}

			set
			{
				lock (settingsLock)
				{
					if (value == this.maxbitrate)
						return;

					this.maxbitrate = value;
					OnPropertyChanged ("MaximumAudioBitrate");
				}
			}
		}

		private int defaultbitrate = 48000;
		public virtual int DefaultAudioBitrate
		{
			get
			{
				lock (settingsLock)
				{
					return this.defaultbitrate;
				}
			}

			set
			{
				lock (settingsLock)
				{
					if (value == this.defaultbitrate)
						return;

					this.defaultbitrate = value;
					OnPropertyChanged ("DefaultAudioBitrate");
				}
			}
		}

		private string serverLogo = null;
		public virtual string ServerLogo
		{
			get
			{
				lock(settingsLock)
				{
					return this.serverLogo;
				}
			}

			set
			{
				lock (settingsLock)
				{
					if (value == this.serverLogo)
						return;

					this.serverLogo = value;
					OnPropertyChanged ("Logo");
				}
			}
		}

		private bool allowGuestLogins = true;
		public virtual bool AllowGuestLogins
		{
			get
			{
				lock (settingsLock)
				{
					return this.allowGuestLogins;
				}
			}

			set
			{
				lock (settingsLock)
				{
					if (value == this.allowGuestLogins)
						return;

					this.allowGuestLogins = value;
					OnPropertyChanged ("AllowGuestLogins");
				}
			}
		}

		private string serverPassword;
		public virtual string ServerPassword
		{
			get
			{
				lock (settingsLock)
				{
					return this.serverPassword;
				}
			}

			set
			{
				lock (settingsLock)
				{
					if (value == this.serverPassword)
						return;

					this.serverPassword = value;
					OnPropertyChanged ("ServerPassword");
				}
			}
		}

		protected readonly object settingsLock = new object();

		protected void OnPropertyChanged (string propertyName)
		{
			var changed = this.PropertyChanged;
			if (changed != null)
				changed (this, new PropertyChangedEventArgs (propertyName));
		}
	}
}