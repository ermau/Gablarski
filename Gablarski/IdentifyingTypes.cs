using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Gablarski
{
	public class IdentifyingTypes
	{
		public IdentifyingTypes (Type userIdType, Type channelIdType)
		{
			if (userIdType == null)
				throw new ArgumentNullException ("userIdType");
			if (channelIdType == null)
				throw new ArgumentNullException ("channelIdType");

			this.UserIdType = userIdType;
			this.WriteUser = GetSerializationMethod (this.UserIdType);
			this.ReadUser = GetDeserializationMethod (this.UserIdType);

			this.ChannelIdType = channelIdType;
			this.WriteChannel = GetSerializationMethod (this.ChannelIdType);
			this.ReadChannel = GetDeserializationMethod (this.ChannelIdType);
		}

		public readonly Action<IValueWriter, object> WriteUser;
		public readonly Func<IValueReader, object> ReadUser;

		/// <summary>
		/// Gets the user identifying type.
		/// </summary>
		public Type UserIdType
		{
			get;
			private set;
		}

		public readonly Action<IValueWriter, object> WriteChannel;
		public readonly Func<IValueReader, object> ReadChannel;

		/// <summary>
		/// Gets the channel identifying type.
		/// </summary>
		public Type ChannelIdType
		{
			get;
			private set;
		}

		internal static Action<IValueWriter, object> GetSerializationMethod (Type idType)
		{
			if (idType == null)
				throw new ArgumentNullException ("idType");

			if (typeof(Guid) == idType)
				return (w, o) => w.WriteBytes (((Guid)o).ToByteArray());

			var method = (from mi in typeof (IValueWriter).GetMethods()
							let param = mi.GetParameters()
							where param.Length == 1 && param[0].ParameterType == idType
							select mi).FirstOrDefault();

			if (method == null)
				throw new ArgumentException("No suitable serializer could be found.");

			var args = new[] { typeof (IValueWriter), typeof (object) };
			var dm = new DynamicMethod ("Serialize", typeof(void), args);
			var gen = dm.GetILGenerator();
			var value = gen.DeclareLocal (idType);

			Label call = gen.DefineLabel();
			Label def = gen.DefineLabel();

			// Check if value is null
			gen.Emit (OpCodes.Ldarg_1);
			gen.Emit (OpCodes.Brfalse, def);

			// Use the default value
			gen.MarkLabel (def);
			gen.Emit (OpCodes.Ldarg_0);
			
			if (idType.IsByRef)
				gen.Emit (OpCodes.Ldnull);
			else
			{
				gen.Emit (OpCodes.Ldloca_S, value);
				gen.Emit (OpCodes.Initobj, idType);
				gen.Emit (OpCodes.Ldloca_S, value);
				gen.Emit (OpCodes.Ldobj, idType);
			}

			gen.Emit (OpCodes.Br, call);

			// Use the supplied value
			gen.Emit (OpCodes.Ldarg_0);
			gen.Emit (OpCodes.Ldarg_1);
			if (idType.IsByRef)
				gen.Emit (OpCodes.Castclass, idType);
			else
				gen.Emit (OpCodes.Unbox_Any, idType);

			// Call the serialization
			gen.MarkLabel (call);
			gen.Emit (OpCodes.Callvirt, method);
			gen.Emit (OpCodes.Ret);

			return (Action<IValueWriter, object>)dm.CreateDelegate (typeof (Action<IValueWriter, object>));
		}

		internal static Func<IValueReader, object> GetDeserializationMethod (Type idType)
		{
			if (idType == null)
				throw new ArgumentNullException ("idType");

			if (typeof(Guid) == idType)
				return r => new Guid (r.ReadBytes());

			var method = typeof (IValueReader).GetMethods().Where (m => m.ReturnType == idType).FirstOrDefault();

			if (method == null)
				throw new ArgumentException("No suitable deserializer could be found.");

			var args = new[] { typeof (IValueReader) };
			var dm = new DynamicMethod ("Deserialize", typeof(object), args);
			var gen = dm.GetILGenerator();
			gen.Emit (OpCodes.Ldarg_0);
			gen.Emit (OpCodes.Callvirt, method);
			gen.Emit (OpCodes.Box, idType);
			gen.Emit (OpCodes.Ret);

			return (Func<IValueReader, object>)dm.CreateDelegate (typeof (Func<IValueReader, object>));
		}
	}
}