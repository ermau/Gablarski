using System.Reflection;
using System.Runtime.InteropServices;
using Gablarski.Audio;
using Gablarski.Clients;
using Gablarski.OpenAL.Providers;

[assembly: AssemblyTitle ("Gablarski.OpenAL")]
[assembly: AssemblyDescription ("")]
[assembly: AssemblyConfiguration ("")]
[assembly: AssemblyCompany ("")]
[assembly: AssemblyProduct ("Gablarski.OpenAL")]
[assembly: AssemblyCopyright ("Copyright © Eric Maupin 2009-2011, Xamarin Inc. 2011-2014")]
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]

[assembly: ComVisible (false)]
[assembly: Guid ("696e1719-72e2-4d90-afc6-f647dc00a422")]

[assembly: AssemblyVersion ("0.0.0.0")]
[assembly: AssemblyFileVersion ("0.0.0.0")]

[assembly: Module (typeof (IAudioPlaybackProvider), typeof (OpenALPlaybackProvider))]
[assembly: Module (typeof (IAudioCaptureProvider), typeof (OpenALPlaybackProvider))]