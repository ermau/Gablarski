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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Gablarski.Audio;
using Gablarski.Clients.Input;
using Gablarski.Clients.Media;

namespace Gablarski.Clients
{
	public static class Modules
	{
		public static void Init (IModuleFinder finder)
		{
			if (finder == null)
				throw new ArgumentNullException ("finder");

			Retrievers.Clear();
			Instances.Clear();

			moduleFinder = finder;

			Prelaunch<IInputProvider>();
			Prelaunch<IAudioPlaybackProvider>();
			Prelaunch<IAudioCaptureProvider>();
			Prelaunch<IMediaPlayer>();
			Prelaunch<INotifier>();
			Prelaunch<ITextToSpeech>();
			Prelaunch<ISpeechRecognizer>();
		}

		public static async Task<TContract> GetImplementerOrDefaultAsync<TContract> (string simpleName)
		{
			if (moduleFinder == null)
				throw new InvalidOperationException ("You must call Modules.Init before using Modules");

			List<object> instances;
			if (!Instances.TryGetValue (typeof (TContract), out instances)) {
				IReadOnlyCollection<Type> types = await GetLoadTask<TContract>().ConfigureAwait (false);
				if (types.Count > 0) {
					instances = Instances.GetOrAdd (typeof (TContract), t => new List<object> { CreateSpecificInstance<TContract> (simpleName, types) });
					return (TContract) instances[0];
				} else
					return default(TContract);
			}

			TContract instance = (TContract)instances.FirstOrDefault (o => o.GetType().GetSimpleName() == simpleName);
			if (Equals (instance, default(TContract))) {
				var types = await GetLoadTask<TContract>().ConfigureAwait (false);
				lock (instances) {
					instance = CreateSpecificInstance<TContract> (simpleName, types);
					instances.Add (instance);
				}
			}

			return instance;
		}

		public static async Task<TContract> GetImplementerAsync<TContract> (string simpleName)
		{
			if (moduleFinder == null)
				throw new InvalidOperationException ("You must call Modules.Init before using Modules");

			TContract instance = await GetImplementerOrDefaultAsync<TContract> (simpleName);
			if (instance.GetType().GetSimpleName() != simpleName)
				return default(TContract);

			return instance;
		}

		public static async Task<IReadOnlyCollection<TContract>> GetImplementersAsync<TContract>()
		{
			if (moduleFinder == null)
				throw new InvalidOperationException ("You must call Modules.Init before using Modules");

			List<TContract> instances = new List<TContract>();

			List<object> existingInstances = Instances.GetOrAdd (typeof (TContract), t => new List<object>());

			HashSet<Type> typesAdded = new HashSet<Type>();

			var types = await GetLoadTask<TContract>().ConfigureAwait (false);
			lock (existingInstances) {
				foreach (Type type in types) {
					if (typesAdded.Contains (type))
						continue;

					TContract instance = existingInstances.Cast<TContract>().FirstOrDefault (i => i.GetType() == type);

					if (Equals (instance, default(TContract)))
						instances.Add ((TContract) Activator.CreateInstance (type));
					else
						instances.Add (instance);

					typesAdded.Add (type);
				}
			}

			return instances;
		}

		public static Task<IReadOnlyCollection<Type>> GetModulesFor<T>()
		{
			if (moduleFinder == null)
				throw new InvalidOperationException ("You must call Modules.Init before using Modules");

			return GetLoadTask<T>();
		}

		private static IModuleFinder moduleFinder;
		private static readonly ConcurrentDictionary<Type, Task<IReadOnlyCollection<Type>>> Retrievers = new ConcurrentDictionary<Type, Task<IReadOnlyCollection<Type>>>();
		private static readonly ConcurrentDictionary<Type, List<object>> Instances = new ConcurrentDictionary<Type, List<object>>();

		private static void Prelaunch<TContract>()
		{
			Retrievers.GetOrAdd (typeof (TContract), t => moduleFinder.LoadExportsAsync<TContract>());
		}

		private static Task<IReadOnlyCollection<Type>> GetLoadTask<TContract>()
		{
			return Retrievers.GetOrAdd (typeof (TContract), t => moduleFinder.LoadExportsAsync<TContract>());
		}

		private static Type GetImplementerType (string simpleName, IEnumerable<Type> modules)
		{
			if (!String.IsNullOrWhiteSpace (simpleName))
				modules = modules.Where (t => t.GetSimpleName() == simpleName);

			Type moduleType = modules.FirstOrDefault();
			if (moduleType == null)
				return null;

			return moduleType;
		}

		private static TContract CreateSpecificInstance<TContract> (string simpleName, IEnumerable<Type> modules)
		{
			Type moduleType = GetImplementerType (simpleName, modules);
			if (moduleType == null)
				return default(TContract);

			return (TContract) Activator.CreateInstance (moduleType);
		}
	}
}