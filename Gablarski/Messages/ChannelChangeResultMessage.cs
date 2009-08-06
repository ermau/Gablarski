using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public enum ChannelChangeResult
		: byte
	{
		/// <summary>
		/// Failed for an unknown reason.
		/// </summary>
		FailedUnknown = 0,

		/// <summary>
		/// Succeeded.
		/// </summary>
		Success = 1,

		/// <summary>
		/// Failed due to insufficient permissions.
		/// </summary>
		FailedPermissions = 2,

		/// <summary>
		/// Failed because the target channel wasn't found.
		/// </summary>
		FailedUnknownChannel = 3
	}

	public class ChannelChangeResultMessage
		: ServerMessage
	{
		public ChannelChangeResultMessage ()
			: base (ServerMessageType.ChangeChannelResult)
		{
		}

		public ChannelChangeResultMessage (ChannelChangeInfo moveInfo)
			: this ()
		{
			this.Result = ChannelChangeResult.Success;
			this.MoveInfo = moveInfo;
		}

		public ChannelChangeResult Result
		{
			get;
			set;
		}

		public ChannelChangeInfo MoveInfo
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteByte ((byte)this.Result);
			
			if (this.Result == ChannelChangeResult.Success)
				this.MoveInfo.Serialize (writer);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Result = (ChannelChangeResult)reader.ReadByte ();

			if (this.Result == ChannelChangeResult.Success)
				this.MoveInfo = new ChannelChangeInfo (reader);
		}
	}
}