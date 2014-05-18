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

using System.Linq;
using NUnit.Framework;

namespace Gablarski.Clients.Core.Tests
{
	[TestFixture]
	public class ErrorManagerTests
	{
		private ErrorManager manager;

		[SetUp]
		public void Setup()
		{
			manager = new ErrorManager();
		}

		[Test]
		public void AddError()
		{
			bool changedRaised = false;
			manager.ErrorsChanged += (sender, args) => {
				changedRaised = true;
				Assert.That (args.PropertyName, Is.EqualTo ("property"));
			};

			manager.AddError ("error", "property");
			Assert.That (manager.HasErrors, Is.True);
			Assert.That (changedRaised, Is.True, "ErrorsChanged was not raised");
		}

		[Test]
		public void AddErrorDuplicate()
		{
			manager.AddError ("error", "property");

			bool changedRaised = false;
			manager.ErrorsChanged += (sender, args) => {
				changedRaised = true;
			};

			manager.AddError ("error", "property");
			Assert.That (manager.HasErrors, Is.True);
			Assert.That (changedRaised, Is.False, "ErrorsChanged was raised");
			Assert.That (manager.GetErrors ("property"), Has.Count.EqualTo (1));
		}

		[Test]
		public void RemoveError()
		{
			AddError();

			bool changedRaised = false;
			manager.ErrorsChanged += (sender, args) => {
				changedRaised = true;
				Assert.That (args.PropertyName, Is.EqualTo ("property"));
			};

			manager.RemoveError ("error", "property");
			Assert.That (manager.HasErrors, Is.False);
			Assert.That (changedRaised, Is.True, "ErrorsChanged was not raised");
		}

		[Test]
		public void RemoveUnknownError()
		{
			bool changedRaised = false;
			manager.ErrorsChanged += (sender, args) => {
				changedRaised = true;
			};

			Assert.That (() => manager.RemoveError ("error", "property"), Throws.Nothing);
			Assert.That (manager.HasErrors, Is.False);
			Assert.That (changedRaised, Is.False, "ErrorsChanged was raised");
		}

		[Test]
		public void GetErrors()
		{
			AddError();

			var errors = manager.GetErrors ("property");
			Assert.That (errors, Is.Not.Null);
			Assert.That (errors.Cast<string>().Single(), Is.EqualTo ("error"));
		}
	}
}