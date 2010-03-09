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
using System.Reflection;
using Cadenza.Collections;

namespace Gablarski.Clients
{
	public class AutomaticErrorReporter
	{
		public AutomaticErrorReporter()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
		}

		/// <summary>
		/// Adds <paramref name="reporter"/> to the list of error reporters.
		/// </summary>
		/// <param name="reporter"></param>
		/// <exception cref="ArgumentNullException"><paramref name="reporter"/> is <c>null</c>.</exception>
		public void Add (IErrorReporter reporter)
		{
			lock (reporters)
			{
				reporters.Add (reporter.AssemblyHandled, reporter);
			}
		}

		private readonly MutableLookup<Assembly, IErrorReporter> reporters = new MutableLookup<Assembly, IErrorReporter>();

		private void CurrentDomainUnhandledException (object sender, UnhandledExceptionEventArgs e)
		{
			var ex = (Exception) e.ExceptionObject;

			lock (reporters)
			{
				foreach (IErrorReporter reporter in reporters[ex.TargetSite.Module.Assembly])
					reporter.ReportError (ex);
			}
		}
	}
}