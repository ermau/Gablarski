using System.Reflection;
using System.Runtime.InteropServices;

using Gablarski.Clients;
using Gablarski.Clients.Media;
using Gablarski.iTunes;

[assembly: AssemblyTitle ("Gablarski.iTunes")]
[assembly: AssemblyDescription ("")]
[assembly: AssemblyConfiguration ("")]
[assembly: AssemblyCompany ("")]
[assembly: AssemblyProduct ("Gablarski.iTunes")]
[assembly: AssemblyCopyright ("Copyright © Eric Maupin 2009-2011, Xamarin Inc. 2014")]
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]

[assembly: ComVisible (false)]
[assembly: Guid ("b3c316be-55b9-4865-ae74-0253ff3219ff")]

[assembly: AssemblyVersion ("0.0.0.0")]
[assembly: AssemblyFileVersion ("0.0.0.0")]

[assembly: Module (typeof (IMediaPlayer), typeof (iTunesIntegration))]
[assembly: Module (typeof (IControlMediaPlayer), typeof (iTunesIntegration))]