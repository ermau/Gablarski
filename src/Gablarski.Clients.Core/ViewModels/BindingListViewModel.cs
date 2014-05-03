//
// BindingListViewModel.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2011, Eric Maupin
// Copyright (c) 2013-2014, Xamarin Inc.
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Gablarski.Clients.Persistence;
using Gablarski.Clients.Input;

namespace Gablarski.Clients.ViewModels
{
	public class BindingListViewModel
		: BusyViewModel
	{
		private readonly IntPtr window;

		public BindingListViewModel (IntPtr window, ICommand recordCommand)
		{
			if (recordCommand == null)
				throw new ArgumentNullException ("recordCommand");

			RecordCommand = recordCommand;
			this.window = window;
			RemoveCommand = new RelayCommand<CommandBindingViewModel> (b => this.bindings.Remove (b));
		}

		private IInputProvider inputProvider;
		public IInputProvider InputProvider
		{
			get { return this.inputProvider; }
			set
			{
				if (this.inputProvider == value)
					return;

				if (this.inputProvider != null)
					this.inputProvider.Detach();

				this.inputProvider = value;
				OnPropertyChanged();

				if (this.inputProvider == null)
					return;

				IsBusy = true;

				this.inputProvider.AttachAsync (this.window).ContinueWith (t => {
					this.bindings = new ObservableCollection<CommandBindingViewModel> (ClientData.GetCommandBindings()
						.Where (b => value.GetType().GetSimpleName() == b.ProviderType)
						.Select (b => new CommandBindingViewModel (value, b)));

					OnPropertyChanged ("Bindings");
					IsBusy = false;
				}, TaskScheduler.FromCurrentSynchronizationContext());
			}
		}

		public IDictionary<string, Command> Commands
		{
			get
			{
				return ((Command[])Enum.GetValues (typeof (Command)))
					.Skip (1)
					.Where (c => !typeof (Command).GetMember (c.ToString())[0].GetCustomAttributes (typeof (CommandParameter), true).Any())
					.ToDictionary (c => SpaceEnum (c.ToString()), c => c);
			}
		}

		public ICollection<CommandBindingViewModel> Bindings
		{
			get { return this.bindings; }
		}

		public ICommand RecordCommand
		{
			get;
			private set;
		}

		public ICommand RemoveCommand
		{
			get;
			private set;
		}

		private ObservableCollection<CommandBindingViewModel> bindings;

		internal static string SpaceEnum (string enumName)
		{
			string name = enumName[0].ToString();
			for (int i = 1; i < enumName.Length; ++i)
			{
				if (Char.IsUpper (enumName[i]))
					name += " ";

				name += enumName[i];
			}

			return name;
		}
	}
}