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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using NUnit.Framework;

namespace Gablarski.Clients.Core.Tests
{
	[TestFixture]
	public class SelectedObservableCollectionTests
	{
		[Test]
		public void CtorNull()
		{
			Assert.That (() => new SelectedObservableCollection<string, string> (null, s => s), Throws.TypeOf<ArgumentNullException>());
			Assert.That (() => new SelectedObservableCollection<string, string> (new string[0], null), Throws.TypeOf<ArgumentNullException>());
		}

		[Test]
		public void Ctor()
		{
			var a = new[] { "foo", "bar" };

			var ob = new SelectedObservableCollection<string, string> (a, s => s + "s");

			Assert.That (ob.Count, Is.EqualTo (2));
			Assert.That (ob, Contains.Item ("foos"));
			Assert.That (ob, Contains.Item ("bars"));
		}

		[Test]
		public void Add()
		{
			var original = new ObservableCollection<string> { "foo", "bar" };
			var ob = new SelectedObservableCollection<string, string> (original, s => s + "s");

			bool raised = false;
			ob.CollectionChanged += (sender, args) => {
				raised = true;
				Assert.That (args.Action, Is.EqualTo (NotifyCollectionChangedAction.Add));
				Assert.That (args.NewItems, Is.Not.Null);
				Assert.That (args.NewItems, Contains.Item ("bazs"));
			};

			original.Add ("baz");

			Assert.That (ob.Count, Is.EqualTo (3));
			Assert.That (ob, Contains.Item ("foos"));
			Assert.That (ob, Contains.Item ("bars"));
			Assert.That (ob, Contains.Item ("bazs"));

			Assert.That (raised, Is.Not.False, "CollectionChanged was not raised");
		}
	}
}
