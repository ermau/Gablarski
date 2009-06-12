using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Mono.Rocks;

namespace Gablarski.Messages
{
	public abstract class MessageBase
	{
		public abstract ushort MessageTypeCode
		{
			get;
		}

		public virtual bool Reliable
		{
			get { return true; }
		}

		public abstract void WritePayload (IValueWriter writer, IdentifyingTypes idTypes);
		public abstract void ReadPayload (IValueReader reader, IdentifyingTypes idTypes);

		public static ReadOnlyDictionary<ushort, Func<MessageBase>> MessageTypes
		{
			get;
			private set;
		}

		static MessageBase ()
		{
			Dictionary<ushort, Func<MessageBase>> messageTypes = new Dictionary<ushort, Func<MessageBase>> ();

			Type msgb = typeof(MessageBase);

			Type[] types = Assembly.GetExecutingAssembly ().GetTypes ();
			foreach (Type t in types.Where (t => msgb.IsAssignableFrom (t) && !t.IsAbstract))
			{
				var ctor = t.GetConstructor (Type.EmptyTypes);

				var dctor = new DynamicMethod (t.Name, msgb, null);
				var il = dctor.GetILGenerator ();

				il.Emit (OpCodes.Newobj, ctor);
				il.Emit (OpCodes.Ret);

				Func<MessageBase> dctord = (Func<MessageBase>)dctor.CreateDelegate (typeof (Func<MessageBase>));
				MessageBase dud = dctord();
				messageTypes.Add (dud.MessageTypeCode, dctord);
			}

			MessageTypes = new ReadOnlyDictionary<ushort, Func<MessageBase>> (messageTypes);
		}
	}
}