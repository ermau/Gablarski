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
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Gablarski.Clients
{
	public static class AvatarCache
	{
		static AvatarCache()
		{
			MysteryMan = new Lazy<Task<byte[]>> (() => WebClient.GetByteArrayAsync (new Uri (GravatarBaseUri, "0000?f=y&d=mm&s=96")));
		}

		/// <summary>
		/// Gets the binary data from the specified URL, or from the cache if available.
		/// </summary>
		/// <param name="avatar">The URL or Gravatar email to retrieve the image from.</param>
		/// <returns>The byte array for the image data. <c>null</c> if <paramref name="avatar"/> is invalid.</returns>
		public static async Task<byte[]> GetAvatarAsync (string avatar)
		{
			if (String.IsNullOrWhiteSpace (avatar))
				return await MysteryMan.Value.ConfigureAwait (false);

			byte[] avatarData;
			if (Avatars.TryGetValue (avatar, out avatarData))
				return avatarData;

			Uri imageUri;
			if (Uri.TryCreate (avatar, UriKind.Absolute, out imageUri) && imageUri.Scheme != "gravatar") {
				if (imageUri.IsFile)
					return null;
			} else {
				if (imageUri != null && imageUri.Scheme == "gravatar") {
					imageUri = new Uri (GravatarBaseUri, imageUri.Host + "?d=mm&s=96");
				} else if (avatar.Contains ("@") && avatar.Contains (".")) {
					string clean = avatar.Trim().ToLower();
					byte[] hashData = Md5.ComputeHash (Encoding.ASCII.GetBytes (clean));
					string hash = hashData.Aggregate (String.Empty, (s, b) => s + b.ToString ("x2"));

					avatar = "gravatar://" + hash;

					imageUri = new Uri (GravatarBaseUri, hash + "?d=mm&s=96");
				} else
					return await MysteryMan.Value.ConfigureAwait (false);
			}

			try {
				byte[] image = await WebClient.GetByteArrayAsync (imageUri).ConfigureAwait (false);
				Avatars.TryAdd (avatar, image);

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

		private static readonly Lazy<Task<byte[]>> MysteryMan;

		private static readonly Uri GravatarBaseUri = new Uri ("http://www.gravatar.com/avatar/");

		private static readonly MD5CryptoServiceProvider Md5 = new MD5CryptoServiceProvider();
		private static readonly ConcurrentDictionary<string, byte[]> Avatars = new ConcurrentDictionary<string, byte[]>();
		private static readonly HttpClient WebClient = new HttpClient();
	}
}