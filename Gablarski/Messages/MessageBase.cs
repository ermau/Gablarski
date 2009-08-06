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

		public virtual int MessageSize
		{
			get { return 0; }
		}

		public abstract void WritePayload (IValueWriter writerm);
		public abstract void ReadPayload (IValueReader reader);

		public static bool GetMessage (ushort messageType, out MessageBase msg)
		{
			msg = null;
			Func<MessageBase> msgctor;
			if (MessageTypes.TryGetValue (messageType, out msgctor))
			{
				msg = msgctor();
				return true;
			}

			return false;
		}

		public static ReadOnlyDictionary<ushort, Func<MessageBase>> MessageTypes
		{
			get;
			private set;
		}

		static MessageBase ()
		{
			MessageTypes = new ReadOnlyDictionary<ushort,Func<MessageBase>> (new Dictionary<ushort, Func<MessageBase>>
			{
				{ (ushort)ServerMessageType.AudioDataReceived, () => new AudioDataReceivedMessage() },
				{ (ushort)ServerMessageType.ChangeChannelResult, () => new ChannelChangeResultMessage() },
				{ (ushort)ServerMessageType.ChannelEditResult, () => new ChannelEditResultMessage() },
				{ (ushort)ServerMessageType.ChannelListReceived, () => new ChannelListMessage() },
				{ (ushort)ServerMessageType.ConnectionRejected, () => new ConnectionRejectedMessage() },
				{ (ushort)ServerMessageType.LoginResult, () => new LoginResultMessage() },
				{ (ushort)ServerMessageType.ServerInfoReceived, () => new ServerInfoMessage() },
				{ (ushort)ServerMessageType.SourceListReceived, () => new SourceListMessage() },
				{ (ushort)ServerMessageType.SourceResult, () => new SourceResultMessage() },
				{ (ushort)ServerMessageType.SourcesRemoved, () => new SourcesRemovedMessage() },
				{ (ushort)ServerMessageType.UserChangedChannel, () => new UserChangedChannelMessage() },
				{ (ushort)ServerMessageType.UserDisconnected, () => new UserDisconnectedMessage() },
				{ (ushort)ServerMessageType.UserListReceived, () => new UserListMessage() },
				{ (ushort)ServerMessageType.UserLoggedIn, () => new UserLoggedInMessage() },

				{ (ushort)ClientMessageType.AudioData, () => new SendAudioDataMessage() },
				{ (ushort)ClientMessageType.ChangeChannel, () => new ChannelChangeMessage() },
				{ (ushort)ClientMessageType.Connect, () => new ConnectMessage() },
				{ (ushort)ClientMessageType.Disconnect, () => new DisconnectMessage() },
				{ (ushort)ClientMessageType.EditChannel, () => new ChannelEditMessage() },
				{ (ushort)ClientMessageType.Login, () => new LoginMessage() },
				{ (ushort)ClientMessageType.RequestChannelList, () => new RequestChannelListMessage() },
				{ (ushort)ClientMessageType.RequestSource, () => new RequestSourceMessage() },
				{ (ushort)ClientMessageType.RequestSourceList, () => new RequestSourceListMessage() },
				{ (ushort)ClientMessageType.RequestUserList, () => new RequestUserListMessage() }
			});

			/*Type msgb = typeof(MessageBase);

			foreach (Type t in Assembly.GetExecutingAssembly ().GetTypes ().Where (t => msgb.IsAssignableFrom (t) && !t.IsAbstract))
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

			MessageTypes = new ReadOnlyDictionary<ushort, Func<MessageBase>> (messageTypes);*/
		}
	}
}