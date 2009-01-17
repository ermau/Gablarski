using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading;
using System.Reflection;

namespace Gablarski
{
	public static class Sources
	{
		public static IMediaSource Create (Type sourceType, int sourceID, IUser user)
		{
			if (sourceType.GetInterface("IMediaSource") == null)
				throw new InvalidOperationException("Not media source type.");

			rwl.EnterUpgradeableReadLock();
			if (!types.ContainsKey (sourceType))
			{
				rwl.EnterWriteLock();
				if (!types.ContainsKey (sourceType))
				{
					Type[] argTypes = new Type[] { typeof(int), typeof(IUser)};

					ConstructorInfo ctor = sourceType.GetConstructor(argTypes);
					if (ctor == null)
						throw new InvalidOperationException("Source constructor not found.");

					DynamicMethod method = new DynamicMethod("Create" + sourceType.Name, sourceType, argTypes);
					ILGenerator gen = method.GetILGenerator();
					gen.Emit(OpCodes.Ldarg_0);
					gen.Emit(OpCodes.Ldarg_1);
					gen.Emit(OpCodes.Newobj, ctor);
					gen.Emit(OpCodes.Ret);

					types.Add(sourceType, (Func<int, IUser, IMediaSource>)method.CreateDelegate(typeof(Func<int, IUser, IMediaSource>)));
				}
				rwl.ExitWriteLock();
			}

			IMediaSource source = types[sourceType] (sourceID, user);
			rwl.ExitUpgradeableReadLock();

			return source;
		}

		private static Dictionary<Type, Func<int, IUser, IMediaSource>> types = new Dictionary<Type, Func<int, IUser, IMediaSource>>();
		private static ReaderWriterLockSlim rwl = new ReaderWriterLockSlim();
	}
}