//
// DirectoryModuleFinder.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Gablarski.Annotations;

namespace Gablarski.Clients.Windows
{
	public sealed class DirectoryModuleFinder
		: IModuleFinder
	{
		public DirectoryModuleFinder ([NotNull] params string[] paths)
		{
			if (paths == null)
				throw new ArgumentNullException ("paths");

			scan = Task.Run (() => ScanAssemblies (paths));
		}

		public DirectoryModuleFinder ([NotNull] IEnumerable<string> paths, [NotNull] IEnumerable<string> filesToIgnore)
		{
			if (paths == null)
				throw new ArgumentNullException ("paths");
			if (filesToIgnore == null)
				throw new ArgumentNullException ("filesToIgnore");

			this.filesToIgnore = new HashSet<string> (filesToIgnore);
			this.scan = Task.Run (() => ScanAssemblies (paths.ToArray()));
		}

		public async Task<IReadOnlyCollection<Type>> LoadExportsAsync<TContract>()
		{
			await this.scan.ConfigureAwait (false);

			List<Type> exports;
			if (!types.TryGetValue (typeof (TContract), out exports))
				return new Type[0];

			return exports;
		}

		private readonly Task scan;
		private readonly ConcurrentDictionary<Type, List<Type>> types = new ConcurrentDictionary<Type, List<Type>>();
		private readonly HashSet<string> filesToIgnore;

		private void ScanAssemblies (string[] paths)
		{
			Parallel.ForEach (paths, d => {
				if (!Directory.Exists (d))
					return;

				string[] files = Directory.GetFiles (d, "*.dll");
				foreach (string file in files) {
					if (this.filesToIgnore != null) {
						string filename = Path.GetFileName (file);
						if (this.filesToIgnore.Contains (filename))
							continue;
					}

					Assembly assembly;
					try {
						assembly = Assembly.LoadFrom (file);
					} catch (Exception) {
						continue;
					}

					foreach (var export in assembly.GetCustomAttributes<ModuleAttribute>()) {
						List<Type> exports = types.GetOrAdd (export.ContractType, t => new List<Type>());
						lock (exports)
							exports.Add (export.ExportedType);
					}
				}
			});
		}
	}
}
