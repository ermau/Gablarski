//
// Modules.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2009-2011, Eric Maupin
// Copyright (c) 2011-2014, Xamarin Inc.
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
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Gablarski.Clients.Core.Tests
{
	[TestFixture]
	public class ModulesTests
	{
		class MockModuleFinder
			: IModuleFinder
		{
			private readonly Dictionary<Type, Type[]> modules;

			public MockModuleFinder (Dictionary<Type, Type[]> modules)
			{
				this.modules = modules;
			}

			public Task<IReadOnlyCollection<Type>> LoadExportsAsync<TContract>()
			{
				Type[] implementers;
				if (!this.modules.TryGetValue (typeof (TContract), out implementers))
					implementers = new Type[0];

				return Task.FromResult<IReadOnlyCollection<Type>> (implementers);
			}
		}

		[Test]
		public void InitNull()
		{
			Assert.That (() => Modules.Init (null), Throws.InstanceOf<ArgumentNullException>());
		}

		[Test]
		public async Task GetImplementerOrDefaultAsync_NoImplementers()
		{
			Modules.Init (new MockModuleFinder (new Dictionary<Type, Type[]>()));

			var result = await Modules.GetImplementerOrDefaultAsync<string> (null);
			Assert.That (result, Is.Null);
		}

		interface IContract
		{
			
		}

		class Implementer
			: IContract
		{
		}

		class Implementer2
			: IContract
		{
		}

		[Test]
		public async Task GetImplementerOrDefaultAsync_ImplementerButNotSpecified()
		{
			Modules.Init (new MockModuleFinder (new Dictionary<Type, Type[]> {
				{ typeof (IContract), new[] { typeof (Implementer) } }
			}));

			var result = await Modules.GetImplementerOrDefaultAsync<IContract> (null);

			Assert.That (result, Is.InstanceOf<Implementer>());
		}

		[Test]
		public async Task GetImplementerOrDefaultAsync_NoneFound_DoesntGiveNullFromImplementersLater()
		{
			Modules.Init (new MockModuleFinder (new Dictionary<Type, Type[]>()));

			var result = await Modules.GetImplementerOrDefaultAsync<IContract> (null);
			Assert.That (result, Is.Null);

			var results = await Modules.GetImplementersAsync<IContract>();
			Assert.That (results, Is.Empty);
		}
	}
}
