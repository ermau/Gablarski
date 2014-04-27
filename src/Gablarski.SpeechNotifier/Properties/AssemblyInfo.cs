using System.Reflection;
using System.Runtime.InteropServices;
using Gablarski.Clients;
using Gablarski.Clients.Input;
using Gablarski.SpeechNotifier;

[assembly: AssemblyTitle ("Gablarski.SpeechNotifier")]
[assembly: AssemblyDescription ("")]
[assembly: AssemblyConfiguration ("")]
[assembly: AssemblyCompany ("")]
[assembly: AssemblyProduct ("Gablarski.SpeechNotifier")]
[assembly: AssemblyCopyright ("Copyright © Eric Maupin 2009-2011, Xamarin Inc. 2011-2014")]
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]

[assembly: ComVisible (false)]
[assembly: Guid ("839bf906-dcb9-46c2-8618-b5bcf2f8cbed")]

[assembly: AssemblyVersion ("0.0.0.0")]
[assembly: AssemblyFileVersion ("0.0.0.0")]

[assembly: Module (typeof (ITextToSpeech), typeof (EventSpeech))]
[assembly: Module (typeof (ISpeechRecognizer), typeof (SpeechRecognizer))]