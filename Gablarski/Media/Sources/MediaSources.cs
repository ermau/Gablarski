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
		public static MediaSourceBase Create (Type sourceType, int sourceId, object ownerId)
		{
			if (sourceType == null)
				throw new ArgumentNullException("sourceType");

			if (sourceType.GetInterface ("MediaSourceBase") == null)
				throw new InvalidOperationException ("Not media source type.");

			MediaSourceBase source = null;
			lock (SourceTypes)
			{
				if (!SourceTypes.ContainsKey (sourceType))
					InitType (sourceType);

				if (SourceTypes.ContainsKey (sourceType))
					source = SourceTypes[sourceType] (sourceId, ownerId);
			}

			return source;
		}

		private static readonly Dictionary<Type, Func<int, object, MediaSourceBase>> SourceTypes = new Dictionary<Type, Func<int, object, MediaSourceBase>> ();

		private static void InitType (Type sourceType)
		{
			lock (SourceTypes)
			{
				if (SourceTypes.ContainsKey (sourceType))
					return;
			
				Type[] argTypes = new [] { typeof (int), typeof(object) };

				ConstructorInfo ctor = sourceType.GetConstructor (argTypes);
				if (ctor == null)
					throw new InvalidOperationException ("Source constructor not found.");

				DynamicMethod method = new DynamicMethod ("Create" + sourceType.Name, sourceType, argTypes);
				ILGenerator gen = method.GetILGenerator ();
				gen.Emit (OpCodes.Ldarg_0);
				gen.Emit (OpCodes.Ldarg_1);
				gen.Emit (OpCodes.Newobj, ctor);
				gen.Emit (OpCodes.Ret);

				SourceTypes.Add (sourceType, (Func<int, object, MediaSourceBase>)method.CreateDelegate (typeof (Func<int, object, MediaSourceBase>)));
			}
		}
	}
}