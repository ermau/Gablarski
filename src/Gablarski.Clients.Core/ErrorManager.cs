// Copyright (c) 2014, Eric Maupin
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cadenza.Collections;

namespace Gablarski.Clients
{
	public sealed class ErrorManager
		: INotifyDataErrorInfo
	{
		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		public bool HasErrors
		{
			get { return this.errors.Count > 0; }
		}

		public void AddError (string propertyName, string error)
		{
			if (error == null)
				throw new ArgumentNullException ("error");

			if (!this.errors[propertyName].Contains (error)) {
				this.errors.Add (propertyName, error);
				OnErrorsChanged (new DataErrorsChangedEventArgs (propertyName));
			}
		}

		public void RemoveError (string propertyName, string error)
		{
			if (error == null)
				throw new ArgumentNullException ("error");

			if (this.errors.Remove (propertyName, error))
				OnErrorsChanged (new DataErrorsChangedEventArgs (propertyName));
		}

		public void ClearErrors()
		{
			foreach (IGrouping<string, string> group in this.errors) {
				ClearErrors (group.Key);
			}
		}

		public void ClearErrors (string propertyName)
		{
			if (this.errors.Remove (propertyName))
				OnErrorsChanged (new DataErrorsChangedEventArgs (propertyName));
		}

		public IEnumerable GetErrors (string propertyName)
		{
			IEnumerable<string> propertyErrors;
			if (this.errors.TryGetValues (propertyName, out propertyErrors))
				return propertyErrors;

			return Enumerable.Empty<string>();
		}

		private readonly MutableLookup<string, string> errors = new MutableLookup<string, string>();

		private void OnErrorsChanged (DataErrorsChangedEventArgs e)
		{
			EventHandler<DataErrorsChangedEventArgs> handler = this.ErrorsChanged;
			if (handler != null)
				handler (this, e);
		}
	}
}