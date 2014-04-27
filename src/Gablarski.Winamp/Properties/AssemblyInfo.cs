using System.Reflection;
using System.Runtime.InteropServices;
using Gablarski.Clients;
using Gablarski.Clients.Media;
using Gablarski.Winamp;

[assembly: AssemblyTitle ("Gablarski.Winamp")]
[assembly: AssemblyDescription ("")]
[assembly: AssemblyConfiguration ("")]
[assembly: AssemblyCompany ("")]
[assembly: AssemblyProduct ("Gablarski.Winamp")]
[assembly: AssemblyCopyright ("Copyright © Eric Maupin 2009-2011, Xamarin Inc. 2011-2014")]
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]

[assembly: ComVisible (false)]
[assembly: Guid ("773a2b1e-998c-4c10-a105-bee8a2459e73")]

[assembly: AssemblyVersion ("0.0.0.0")]
[assembly: AssemblyFileVersion ("0.0.0.0")]

[assembly: Module (typeof (IMediaPlayer), typeof (WinampIntegration))]