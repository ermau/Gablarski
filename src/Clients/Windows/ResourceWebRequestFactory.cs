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
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Resources;
using Gablarski.Clients.Windows.Properties;

namespace Gablarski.Clients.Windows
{
	class ResourceWebRequestFactory
		: IWebRequestCreate
	{
		public WebRequest Create (Uri uri)
		{
			return new ResourceRequest (uri);
		}

		private const string Scheme = "resx";

		private class ResourceRequest
			: WebRequest
		{
			private readonly Uri uri;

			public ResourceRequest (Uri uri)
			{
				if (uri == null)
					throw new ArgumentNullException ("uri");

				this.uri = uri;
			}

			public override WebResponse GetResponse()
			{
				return new ResourceResponse (this.uri);
			}
		}

		private class ResourceResponse
			: WebResponse
		{
			private readonly Uri uri;

			public ResourceResponse (Uri uri)
			{
				if (uri == null)
					throw new ArgumentNullException ("uri");

				this.uri = uri;
			}

			public Uri Uri
			{
				get { return this.uri; }
			}

			public override Stream GetResponseStream()
			{
				Resources.ResourceManager.IgnoreCase = true;
				object obj = Resources.ResourceManager.GetObject (Uri.Host);
				if (obj == null)
					return null;

				Bitmap bmp = (Bitmap) obj;

				var stream = new MemoryStream();
				bmp.Save (stream, bmp.RawFormat);
				bmp.Dispose();
				stream.Position = 0;
				return stream;
			}
		}

		public static void Register()
		{
			WebRequest.RegisterPrefix (Scheme, new ResourceWebRequestFactory());
		}
	}
}
