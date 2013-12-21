// Copyright (c) 2013, Eric Maupin
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
using System.Threading;
using System.Windows.Input;
using Gablarski.Clients.Input;

namespace Gablarski.Clients.ViewModels
{
	public class InputSettingsViewModel
		: ViewModelBase
	{
		public InputSettingsViewModel (IntPtr windowHandle)
		{
			InputProviders = Modules.Input;
			
			Bindings = new BindingListViewModel (windowHandle, new RelayCommand<CommandBindingViewModel> (Record, CanRecord));
			Bindings.InputProvider = InputProviders.FirstOrDefault (p => p.GetType().GetSimpleName() == Settings.InputProvider);

			NewBinding = new RelayCommand (() => Bindings.Bindings.Add (new CommandBindingViewModel (Bindings.InputProvider)));
		}

		private bool isRecording;
		public bool IsRecording
		{
			get { return this.isRecording; }
			set
			{
				if (this.isRecording == value)
					return;

				this.isRecording = value;
				OnPropertyChanged();
				RecordCommand.RaiseCanExecuteChanged();
			}
		}

		public IEnumerable<IInputProvider> InputProviders
		{
			get;
			private set;
		}

		public BindingListViewModel Bindings
		{
			get;
			private set;
		}

		public ICommand NewBinding
		{
			get;
			private set;
		}

		public void Close()
		{
			Bindings.InputProvider = null;
		}

		public void UpdateSettings()
		{
			if (Bindings.InputProvider != null) {
				Settings.InputProvider = Bindings.InputProvider.GetType().GetSimpleName();
				Settings.CommandBindings.Clear();

				foreach (var b in Bindings.Bindings)
					Settings.CommandBindings.Add (new Gablarski.Clients.Input.CommandBinding (Bindings.InputProvider, b.Command,
						b.Input));
			} else
				Settings.InputProvider = String.Empty;
		}

		private RelayCommand<CommandBindingViewModel> RecordCommand
		{
			get { return (RelayCommand<CommandBindingViewModel>) Bindings.RecordCommand; }
		}

		private bool CanRecord (CommandBindingViewModel binding)
		{
			return (binding != null && !binding.Recording);
		}

		private readonly object inputSync = new object();
		private CommandBindingViewModel recordingEntry;
		private void Record (CommandBindingViewModel binding)
		{
			IsRecording = true;
			
			this.recordingEntry = binding;
			this.recordingEntry.Recording = true;
			
			Bindings.InputProvider.NewRecording += OnNewRecording ;
			Bindings.InputProvider.BeginRecord();
		}

		private void OnNewRecording (object sender, RecordingEventArgs e)
		{
			if (this.recordingEntry == null)
				return;

			CommandBindingViewModel entry;
			lock (this.inputSync) {
				if (this.recordingEntry == null)
					return;

				e.Provider.NewRecording -= OnNewRecording;
				e.Provider.EndRecord();

				entry = Interlocked.Exchange (ref this.recordingEntry, null);
				entry.Input = e.RecordedInput;
				entry.ProviderType = e.Provider.GetType().Name;
				entry.Recording = false;
			}

			IsRecording = false;
		}
	}
}
