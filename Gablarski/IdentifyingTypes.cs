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
			if (typeof(Guid) == idType)
				return (w, o) => w.WriteBytes (((Guid)o).ToByteArray());

			var method = (from mi in typeof (IValueWriter).GetMethods()
							let param = mi.GetParameters()
							where param.Length == 1 && param[0].ParameterType == idType
							select mi).FirstOrDefault();

			if (method == null)
				throw new ArgumentException("No suitable serializer could be found.");

			var args = new[] { typeof (IValueWriter), typeof (object) };
			var dm = new DynamicMethod ("SerializeUser", typeof(void), args);
			var gen = dm.GetILGenerator();
			gen.Emit (OpCodes.Ldarg_0);
			gen.Emit (OpCodes.Ldarg_1);
			gen.EmitCall (OpCodes.Call, method, null);

			return (Action<IValueWriter, object>)dm.CreateDelegate (typeof (Action<IValueWriter, object>));
		}

		internal static Func<IValueReader, object> GetDeserializationMethod (Type idType)
		{
			if (typeof(Guid) == idType)
				return r => new Guid (r.ReadBytes());

			var method = typeof (IValueReader).GetMethods().Where (m => m.ReturnType == idType).FirstOrDefault();

			if (method == null)
				throw new ArgumentException("No suitable deserializer could be found.");

			var args = new[] { typeof (IValueReader) };
			var dm = new DynamicMethod ("SerializeUser", typeof(object), args);
			var gen = dm.GetILGenerator();
			gen.Emit (OpCodes.Ldarg_0);
			gen.EmitCall (OpCodes.Call, method, null);
			gen.Emit (OpCodes.Ret);

			return (Func<IValueReader, object>)dm.CreateDelegate (typeof (Func<IValueReader, object>));
		}
	}
}