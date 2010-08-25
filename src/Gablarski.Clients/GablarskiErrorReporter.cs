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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml.Linq;
using Gablarski.Client;

namespace Gablarski.Clients
{
	public class GablarskiErrorReporter
		: IErrorReporter
	{
		private static readonly string OperatingSystemIdentifier;

		static GablarskiErrorReporter()
		{
			OperatingSystemIdentifier = GetOS();
		}

		public IEnumerable<Assembly> AssembliesHandled
		{
			get { return new[] { typeof (GablarskiClient).Assembly, typeof (GablarskiErrorReporter).Assembly }; }
		}

		public void ReportError (Exception ex)
		{
			throw new NotImplementedException();
		}

		private const string SpaceURL = "http://www.assembla.com/spaces/gablarski/tickets";
		private static readonly WebClient web = new WebClient();

		private static XElement GetDuplicate (Exception ex)
		{
			throw new NotImplementedException();

			/*string description = ex.GetType();

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (SpaceURL);
			request.Accept = "Accept: application/xml";
			
			using (WebResponse response = request.GetResponse())
			using (StreamReader reader = new StreamReader (response.GetResponseStream()))
			{
				XElement ticket = XDocument.Load (reader).Descendants ("ticket").FirstOrDefault (x => x.Attribute ("description").Value == description);
			}*/
		}

		private static string TraverseError (Exception ex)
		{
			throw new NotImplementedException();
		}

		private static string GetOS()
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Unix:
				case PlatformID.MacOSX:
					try
					{
						ProcessStartInfo sw_vers = new ProcessStartInfo("sw_vers");
						sw_vers.RedirectStandardOutput = true;
						using (var p = Process.Start (sw_vers))
						{
							p.StandardOutput.ReadLine();
							string version = p.StandardOutput.ReadLine();
							string[] parts = version.Split ('.');

							return String.Format ("OSX {0}.{1}", parts[0], parts[1]);
						}
					}
					catch
					{
						return "Linux";
					}

					break;

				default:
					string sixtyfour = (IntPtr.Size == 8) ? " x64" : String.Empty;
					if (Environment.OSVersion.Version.Major == 5)
						return "XP" + sixtyfour;
					else if (Environment.OSVersion.Version.Minor == 0)
						return "Vista" + sixtyfour;
					else if (Environment.OSVersion.Version.Major == 1)
						return "Windows 7" + sixtyfour;

					break;
			}

			return String.Empty;
		}
	}
}