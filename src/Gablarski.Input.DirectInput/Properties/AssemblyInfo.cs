using System.Reflection;
using System.Runtime.InteropServices;
using Gablarski.Clients;
using Gablarski.Clients.Input;
using Gablarski.Input.DirectInput;

[assembly: AssemblyTitle ("Gablarski.Input.DirectInput")]
[assembly: AssemblyDescription ("")]
[assembly: AssemblyConfiguration ("")]
[assembly: AssemblyCompany ("")]
[assembly: AssemblyProduct ("Gablarski.Input.DirectInput")]
[assembly: AssemblyCopyright ("Copyright © Eric Maupin 2009-2011, Xamarin Inc. 2011-2014")]
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]

[assembly: ComVisible (false)]
[assembly: Guid ("4510ed9d-d0b6-4d3d-a471-51543a0844ad")]

[assembly: AssemblyVersion ("0.0.0.0")]
[assembly: AssemblyFileVersion ("0.0.0.0")]

[assembly: Module (typeof (IInputProvider), typeof (DirectInputProvider))]