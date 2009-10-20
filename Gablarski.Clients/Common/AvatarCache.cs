using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Gablarski.Client;
using Gablarski.Network;

namespace Gablarski.Clients.Windows
{
	public static class AvatarCache
	{
		/// <summary>
		/// Gets an <see cref="Bitmap"/> for the specified from the URL, from the cache if available.
		/// </summary>
		/// <param name="url">The URL to retrieve the image from.</param>
		/// <returns>The <see cref="Bitmap"/> for the url. <c>null</c> if <paramref name="url"/> is invalid.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="url"/> is null.</exception>
		public static Bitmap GetAvatar (string url)
		{
			if (url == null)
				throw new ArgumentNullException("url");
			
			if (url.Trim() == String.Empty)
				return null;

			lock (Avatars)
			{
				if (Avatars.ContainsKey (url))
					return Avatars[url];
			}

			Uri imageUri;
			try
			{
				imageUri = new Uri (url);
				if (imageUri.IsFile)
					return null;
			}
			catch (FormatException)
			{
				return null;
			}

			byte[] image = wclient.DownloadData (imageUri);

			Bitmap avatar;
			try
			{
				avatar = new Bitmap (new MemoryStream (image));
			}
			catch (ArgumentException)
			{
				return null;
			}

			lock (Avatars)
			{
				Avatars.Add (url, avatar);
			}

			return avatar;
		}

		private static readonly Dictionary<string, Bitmap> Avatars = new Dictionary<string, Bitmap>();
		private static readonly WebClient wclient = new WebClient();
	}
}