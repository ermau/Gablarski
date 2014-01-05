// Copyright (c) 2013-2014, Eric Maupin
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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Gablarski.Annotations;

namespace Gablarski.Clients
{
	public sealed class AsyncValue<T>
		: INotifyPropertyChanged
	{
		private readonly bool incompleteEnabled;
		private readonly T incompleteValue;
		private readonly Task<T> valueTask;
		private readonly T defaultValue;
		private bool isRunning = true;

		public AsyncValue ([NotNull] Task<T> valueTask, T defaultValue)
		{
			if (valueTask == null)
				throw new ArgumentNullException ("valueTask");

			this.valueTask = valueTask;
			this.defaultValue = defaultValue;

			this.valueTask.ContinueWith (t => {
				IsRunning = false;
			}, TaskContinuationOptions.HideScheduler);

			this.valueTask.ContinueWith (t => {
				OnPropertyChanged ("Value");
			}, TaskContinuationOptions.HideScheduler | TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		public AsyncValue ([NotNull] Task<T> valueTask, T defaultValue, T incompleteValue)
			: this (valueTask, defaultValue)
		{
			this.incompleteValue = incompleteValue;
			this.incompleteEnabled = true;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public bool IsRunning
		{
			get { return this.isRunning; }
			set
			{
				if (this.isRunning == value)
					return;
				
				this.isRunning = value;
				OnPropertyChanged();
			}
		}

		public T Value
		{
			get
			{
				if (this.incompleteEnabled && (this.valueTask.IsFaulted || this.valueTask.IsCanceled))
					return this.incompleteValue;
				if (this.valueTask.Status != TaskStatus.RanToCompletion)
					return this.defaultValue;

				return this.valueTask.Result;
			}
		}

		private void OnPropertyChanged ([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler (this, new PropertyChangedEventArgs (propertyName));
		}
	}
}