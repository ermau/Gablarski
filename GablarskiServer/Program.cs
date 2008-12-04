using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Lidgren.Network;
using Gablarski.Server;
using Gablarski.Client;

namespace Gablarski.Server
{
	class Program
	{
		public static NetServer Server;
		public static IAuthProvider Authentication;

		static void Main (string[] args)
		{
			Console.WriteLine ("Gablarski Server v" + Assembly.GetExecutingAssembly ().GetName ().Version + " starting up...");

			Console.WriteLine ("Populating message handlers...");
			Dictionary<ClientMessages, Action<NetConnection, NetBuffer>> handlers = new Dictionary<ClientMessages, Action<NetConnection, NetBuffer>>
			{
				{ ClientMessages.Connect, ConnectionHandler }
			};

			NetConfiguration config = new NetConfiguration ("Gablarski");
			config.MaxConnections = 128;
			config.Port = 6112;

			Console.WriteLine ("Starting listener...");

			Server = new NetServer (config);
			Server.Start ();

			NetBuffer buffer = Server.CreateBuffer();
			
			while (true)
			{
				NetConnection sender;
				NetMessageType type;
				while (Server.ReadMessage (buffer, out type, out sender))
				{
					switch (type)
					{
						case NetMessageType.StatusChanged:
						{
							if (sender.Status == NetConnectionStatus.Connected)
								sender.Tag = new UserConnection (GenerateHash (), Server);

							ConnectionHandler (sender, buffer);

							break;
						}

						case NetMessageType.Data:
							byte sanity = buffer.ReadByte ();
							if (sanity != ServerMessage.FirstByte)
								continue;

							ClientMessages messageType = (ClientMessages)buffer.ReadVariableUInt32 ();

							if (handlers.ContainsKey (messageType))
								handlers[messageType] (sender, buffer);

							break;
					}
				}
			}
		}

		static int GenerateHash ()
		{
			return DateTime.Now.Millisecond + DateTime.Now.Second + 42;
		}

		static void ConnectionHandler (NetConnection connection, NetBuffer buffer)
		{
			ServerMessage msg = new ServerMessage ((UserConnection)connection.Tag);
			msg.MessageType = ServerMessages.Connected;
			msg.Send (Server, connection, NetChannel.ReliableInOrder1);
		}
	}
}