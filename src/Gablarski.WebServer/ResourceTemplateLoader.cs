// Copyright (c) 2009, Eric Maupin
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HttpServer.Rendering;

namespace Gablarski.WebServer
{
	public class ResourceTemplateLoader
		: ITemplateLoader
	{
		public ResourceTemplateLoader()
		{
			ResourceNames = new HashSet<string> (WebAssembly.GetManifestResourceNames());
		}

		/// <summary>
		/// Load a template into a <see cref="T:System.IO.TextReader"/> and return it.
		/// </summary>
		/// <param name="path">Relative path (and filename) to template.</param>
		/// <returns>
		/// a <see cref="T:System.IO.TextReader"/> if file was found; otherwise null.
		/// </returns>
		public TextReader LoadTemplate (string path)
		{
			var rs = WebAssembly.GetManifestResourceStream (ResourcePrefix + path);
			return rs != null ? new StreamReader (rs) : null;
		}

		/// <summary>
		/// Fetch all files from the resource that matches the specified arguments.
		/// </summary>
		/// <param name="path">Where the file should reside.</param>
		/// <param name="filename">Files to check</param>
		/// <returns> a list of files if found; or an empty array if no files are found.</returns>
		public string[] GetFiles (string path, string filename)
		{
			if (filename.EndsWith(".*"))
				filename = path + ".html";

			string[] files = ResourceNames.Select (n => n.Replace (ResourcePrefix, String.Empty)).Where (n => n == filename).ToArray();
			return files;
		}

		/// <summary>
		/// Check's whether a template should be reloaded or not.
		/// </summary>
		/// <param name="info">template information</param>
		/// <returns>
		/// true if template is OK; false if it do not exist or are old.
		/// </returns>
		public bool CheckTemplate (ITemplateInfo info)
		{
			return HasTemplate (info.Filename);
		}

		/// <summary>
		/// Returns whether or not the loader has an instance of the file requested
		/// </summary>
		/// <param name="filename">The name of the template/file</param>
		/// <returns>
		/// True if the loader can provide the file
		/// </returns>
		public bool HasTemplate (string filename)
		{
			if (filename.Contains("\\index.*"))
				filename = filename.Replace ("\\index.*", ".html");

			return ResourceNames.Contains (ResourcePrefix + filename);
		}

		private const string ResourcePrefix = "Gablarski.WebServer.Html.";
		private readonly HashSet<string> ResourceNames;
		private readonly static Assembly WebAssembly = Assembly.GetAssembly(typeof(ResourceTemplateLoader));
	}
}