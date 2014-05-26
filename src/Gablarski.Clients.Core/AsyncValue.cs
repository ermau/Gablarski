//
// AsyncValue.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2013 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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
