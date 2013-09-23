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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Gablarski.Clients.ViewModels
{
	public static class Messenger
	{
		public static void Register<T> (Action<T> responder)
		{
			if (responder == null)
				throw new ArgumentNullException ("responder");

			Action<object> realResponder;
			if (SynchronizationContext.Current == null)
				realResponder = o => responder ((T)o);
			else {
				SynchronizationContext context = SynchronizationContext.Current;
				realResponder = o => {
					context.Send (s => responder ((T)s), o);
				};
			}

			List<Action<object>> respondersForType = Responders.GetOrAdd (typeof (T), t => new List<Action<object>>());
			lock (respondersForType)
				respondersForType.Add (realResponder);
		}

		public static void Send<T> (T message)
		{
			List<Action<object>> respondersForType;
			if (!Responders.TryGetValue (typeof (T), out respondersForType))
				return;

			lock (respondersForType) {
				foreach (Action<object> action in respondersForType) {
					action (message);
				}
			}
		}

		private static readonly ConcurrentDictionary<Type, List<Action<object>>> Responders = new ConcurrentDictionary<Type, List<Action<object>>>();
	}
}
