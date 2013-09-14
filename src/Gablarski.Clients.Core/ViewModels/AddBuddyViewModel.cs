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
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Tempest.Social;

namespace Gablarski.Clients.ViewModels
{
	public sealed class AddBuddyViewModel
		: ViewModelBase
	{
		private readonly SocialClient client;

		public AddBuddyViewModel (SocialClient client)
		{
			if (client == null)
				throw new ArgumentNullException ("client");

			this.client = client;
			this.addBuddy = new RelayCommand<Person> (OnAddBuddy, CanAddBuddy);
			this.searchResults = new AsyncValue<IEnumerable<Person>> (
				Task.FromResult (Enumerable.Empty<Person>()), Enumerable.Empty<Person>());
		}

		public string Search
		{
			get { return this.search; }
			set
			{
				if (Set (() => Search, ref this.search, value))
					RunSearch (value);
			}
		}

		public AsyncValue<IEnumerable<Person>> SearchResults
		{
			get { return this.searchResults; }
			set { Set (() => SearchResults, ref this.searchResults, value); }
		}

		public ICommand AddBuddy
		{
			get { return this.addBuddy; }
		}

		private string search;
		private AsyncValue<IEnumerable<Person>> searchResults;
		private readonly RelayCommand<Person> addBuddy;

		private bool CanAddBuddy (Person person)
		{
			return (person != null);
		}

		private void OnAddBuddy (Person person)
		{
			this.client.WatchList.Add (person);
		}

		private void RunSearch (string value)
		{
			if (String.IsNullOrWhiteSpace (value))
				return;

			SearchResults = new AsyncValue<IEnumerable<Person>> (this.client.SearchAsync (value), Enumerable.Empty<Person>());
		}
	}
}
