using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	/// <summary>
	/// Generic permission denied message
	/// </summary>
	/// <remarks>
	/// Not all messages need an explicit result message,
	/// so this message will be used for any that don't but
	/// still need permissions.
	/// </remarks>
	public class PermissionDeniedMessage
		: ServerMessage
	{
		public PermissionDeniedMessage()
			: base (ServerMessageType.PermissionDenied)
		{
		}

		public PermissionDeniedMessage (ClientMessageType type)
			: this()
		{
			this.DeniedMessage = type;
		}

		public ClientMessageType DeniedMessage
		{
			get; set;
		}

		#region Overrides of MessageBase

		public override void WritePayload(IValueWriter writerm)
		{
			writerm.WriteUInt16 ((ushort)this.DeniedMessage);
		}

		public override void ReadPayload(IValueReader reader)
		{
			this.DeniedMessage = (ClientMessageType)reader.ReadUInt16();
		}

		#endregion
	}
}
