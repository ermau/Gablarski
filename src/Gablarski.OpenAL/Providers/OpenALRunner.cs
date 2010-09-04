// Copyright (c) 2010, Eric Maupin
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
using System.Threading;

namespace Gablarski.OpenAL.Providers
{
	internal static class OpenALRunner
	{
		internal static void AddUser()
		{
			OpenAL.Debug ("Adding OpenAL user");

			int now = Interlocked.Increment (ref counter);

			if (now != 1 || running)
				return;

			lock (SyncRoot)
			{
				if (running)
					return;

				running = true;
				(runnerThread = new Thread (Runner)
				{
					IsBackground = true,
					Name = "OpenAL Runner"
				}).Start();
			}
		}

		internal static void RemoveUser()
		{
			OpenAL.Debug ("Removing OpenAL user");

			int now = Interlocked.Decrement (ref counter);

			if (now != 0 || !running)
				return;

			lock (SyncRoot)
			{
				if (!running)
					return;

				running = false;
				runnerThread.Join();
			}
		}

		internal static void AddPlaybackProvider (OpenALPlaybackProvider provider)
		{
			OpenAL.Debug ("Adding OpenAL Playback Provider");

			if (provider == null)
				throw new ArgumentNullException ("provider");

			lock (PlaybackProviders)
				PlaybackProviders.Add (provider);
		}

		internal static void RemovePlaybackProvider (OpenALPlaybackProvider provider)
		{
			OpenAL.Debug ("Removing OpenAL Playback Provider");

			if (provider == null)
				throw new ArgumentNullException ("provider");

			lock (PlaybackProviders)
				PlaybackProviders.Remove (provider);
		}

		internal static void AddCaptureProvider (OpenALCaptureProvider provider)
		{
			OpenAL.Debug ("Adding OpenAL Capture Provider");

			if (provider == null)
				throw new ArgumentNullException ("provider");

			lock (CaptureProviders)
				CaptureProviders.Add (provider);
		}

		internal static void RemoveCaptureProvider (OpenALCaptureProvider provider)
		{
			OpenAL.Debug ("Removing OpenAL Capture Provider");

			if (provider == null)
				throw new ArgumentNullException ("provider");

			lock (CaptureProviders)
				CaptureProviders.Remove (provider);
		}

		private static readonly object SyncRoot = new object();
		private static int counter;
		private static readonly List<OpenALPlaybackProvider> PlaybackProviders = new List<OpenALPlaybackProvider>();
		private static readonly List<OpenALCaptureProvider> CaptureProviders = new List<OpenALCaptureProvider>();
		private static volatile bool running;
		private static Thread runnerThread;

		private static void Runner()
		{
			while (running)
			{
				UpdateCapture();
				UpdatePlayback();

				Thread.Sleep (1);
			}
		}

		private static void UpdatePlayback()
		{
			if (PlaybackProviders.Count == 0)
				return;

			lock (PlaybackProviders)
			{
				for (int i = 0; i < PlaybackProviders.Count; ++i)
					PlaybackProviders[i].Tick();
			}
		}

		private static void UpdateCapture()
		{
			if (CaptureProviders.Count == 0)
				return;

			lock (CaptureProviders)
			{
				for (int i = 0; i < CaptureProviders.Count; ++i)
					CaptureProviders[i].Tick();
			}
		}
	}
}