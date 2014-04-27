//
// CompositeModuleFinderTests.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2014, Xamarin Inc.
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
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Gablarski.Clients.Core.Tests
{
	[TestFixture]
	public class CompositeModuleFinderTests
	{
		[Test]
		public void CtorNull()
		{
			Assert.That (() => new CompositeModuleFinder (null), Throws.InstanceOf<ArgumentNullException>());
		}

		class MockModuleFinder
			: IModuleFinder
		{
			private readonly Type[] typesToFind;

			public MockModuleFinder (params Type[] typesToFind)
			{
				this.typesToFind = typesToFind;
			}

			public Task<IReadOnlyCollection<Type>> LoadExportsAsync<TContract>()
			{
				return Task.FromResult<IReadOnlyCollection<Type>> (this.typesToFind);
			}
		}

		[Test]
		public async Task LoadExports()
		{
			var finder1 = new MockModuleFinder (typeof (MockModuleFinder));
			var finder2 = new MockModuleFinder (typeof (CompositeModuleFinder));

			var composite = new CompositeModuleFinder (finder1, finder2);

			var exports = await composite.LoadExportsAsync<IModuleFinder>();
			Assert.That (exports, Contains.Item (typeof (MockModuleFinder)));
			Assert.That (exports, Contains.Item (typeof (CompositeModuleFinder)));
		}

		[Test]
		public async Task LoadExportsNoDuplicates()
		{
			var finder1 = new MockModuleFinder (typeof (MockModuleFinder), typeof (CompositeModuleFinder));
			var finder2 = new MockModuleFinder (typeof (CompositeModuleFinder), typeof (MockModuleFinder));

			var composite = new CompositeModuleFinder (finder1, finder2);

			var exports = await composite.LoadExportsAsync<IModuleFinder>();
			Assert.That (exports, Contains.Item (typeof (MockModuleFinder)));
			Assert.That (exports, Contains.Item (typeof (CompositeModuleFinder)));
			Assert.That (exports, new UniqueItemsConstraint());
		}
	}
}
