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
using System.Linq;
using System.Windows.Input;

namespace Gablarski.Clients.ViewModels
{
	public sealed class RelayCommand<T>
		: ICommand
	{
		#if XAMARIN || NETFX_CORE
		public event EventHandler CanExecuteChanged;
		#else
		public event EventHandler CanExecuteChanged
		{
			add {
				if (this.canExecute != null)
					CommandManager.RequerySuggested += value;
			}

			remove {
				if (this.canExecute != null)
					CommandManager.RequerySuggested -= value;
			}
		}
		#endif

		public RelayCommand (Action<T> execute)
		{
			if (execute == null)
				throw new ArgumentNullException ("execute");

			this.execute = execute;
		}

		public RelayCommand (Action<T> execute, Func<T, bool> canExecute)
			: this (execute)
		{
			if (canExecute == null)
				throw new ArgumentNullException ("canExecute");

			this.canExecute = canExecute;
		}

		public bool CanExecute (object parameter)
		{
			if (!(parameter is T))
				return false;

			return (this.canExecute == null) || this.canExecute ((T)parameter);
		}

		public void Execute (object parameter)
		{
			this.execute ((T)parameter);
		}

		public void RaiseCanExecuteChanged()
		{
			#if XAMARIN || NETFX_CORE
			var changed = CanExecuteChanged;
			if (changed != null)
				changed (this, EventArgs.Empty);
			#else
			CommandManager.InvalidateRequerySuggested();
			#endif
		}

		private readonly Action<T> execute;
		private readonly Func<T, bool> canExecute;
	}

	public sealed class RelayCommand
		: ICommand
	{
		public event EventHandler CanExecuteChanged;

		public RelayCommand (Action execute)
		{
			if (execute == null)
				throw new ArgumentNullException ("execute");

			this.execute = execute;
		}

		public RelayCommand (Action execute, Func<bool> canExecute)
			: this (execute)
		{
			if (canExecute == null)
				throw new ArgumentNullException ("canExecute");
			this.canExecute = canExecute;
		}

		public bool CanExecute (object parameter)
		{
			return (this.canExecute == null) || this.canExecute();
		}

		public void Execute (object parameter)
		{
			this.execute();
		}

		public void RaiseCanExecuteChanged()
		{
			var changed = CanExecuteChanged;
			if (changed != null)
				changed (this, EventArgs.Empty);
		}

		private readonly Action execute;
		private readonly Func<bool> canExecute;
	}
}
