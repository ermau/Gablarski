using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Gablarski.Media.Sources
{
	/// <summary>
	/// Media Sources
	/// </summary>
	/// <remarks>
	/// All static members of this class are gauranteed to be thread-safe.
	/// </remarks>
	public static class MediaSources
	{
		public static IMediaSource Create (Type sourceType, int sourceId)
		{
			if (sourceType == null)
				throw new ArgumentNullException("sourceType");

			if (sourceType.GetInterface ("IMediaSource") == null)
				throw new InvalidOperationException ("Not media source type.");

			IMediaSource source = null;
			lock (SourceTypes)
			{
				if (!SourceTypes.ContainsKey (sourceType))
					InitType (sourceType);

				if (SourceTypes.ContainsKey (sourceType))
					source = SourceTypes[sourceType] (sourceId);
			}

			return source;
		}

		private static readonly Dictionary<Type, Func<int, IMediaSource>> SourceTypes = new Dictionary<Type, Func<int, IMediaSource>> ();

		private static void InitType (Type sourceType)
		{
			lock (SourceTypes)
			{
				if (SourceTypes.ContainsKey (sourceType))
					return;
			
				Type[] argTypes = new Type[] { typeof (int) };

				ConstructorInfo ctor = sourceType.GetConstructor (argTypes);
				if (ctor == null)
					throw new InvalidOperationException ("Source constructor not found.");

				DynamicMethod method = new DynamicMethod ("Create" + sourceType.Name, sourceType, argTypes);
				ILGenerator gen = method.GetILGenerator ();
				gen.Emit (OpCodes.Ldarg_0);
				gen.Emit (OpCodes.Newobj, ctor);
				gen.Emit (OpCodes.Ret);

				SourceTypes.Add (sourceType, (Func<int, IMediaSource>)method.CreateDelegate (typeof (Func<int, IMediaSource>)));
			}
		}
	}
}