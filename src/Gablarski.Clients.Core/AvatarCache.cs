// Copyright (c) 2011-2013, Eric Maupin
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
using System.Net.Http;
using System.Threading.Tasks;

namespace Gablarski.Clients
{
	public static class AvatarCache
	{
		/// <summary>
		/// Gets the binary data from the specified URL, or from the cache if available.
		/// </summary>
		/// <param name="url">The URL to retrieve the image from.</param>
		/// <returns>The byte array for the data. <c>null</c> if <paramref name="url"/> is invalid.</returns>
		public static async Task<byte[]> GetAvatarAsync (string url)
		{
			if (String.IsNullOrWhiteSpace (url))
				return null;

			byte[] avatar;
			if (Avatars.TryGetValue (url, out avatar))
				return avatar;

			Uri imageUri;
			if (Uri.TryCreate (url, UriKind.Absolute, out imageUri)) {
				if (imageUri.IsFile)
					return null;
			} else
				return null;

			byte[] image = null;
			try {
				image = await WebClient.GetByteArrayAsync (imageUri).ConfigureAwait (false);
				Avatars.TryAdd (url, image);

				return image;
			} catch {
				return null;
			}
		}

		public static void RemoveAvatar (string url)
		{
			byte[] data;
			Avatars.TryRemove (url, out data);
		}

		public static void Clear()
		{
			Avatars.Clear();
		}

		private static readonly ConcurrentDictionary<string, byte[]> Avatars = new ConcurrentDictionary<string, byte[]>();
		private static readonly HttpClient WebClient = new HttpClient();
	}
}