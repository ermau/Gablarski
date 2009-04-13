using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class TokenResultMessage
		: ServerMessage
	{
		public TokenResultMessage ()
			: base (ServerMessageType.TokenResult)
		{
		}

		public TokenResultMessage (TokenResult result, int token)
			: base (ServerMessageType.TokenResult)
		{
			this.Result = result;
			this.Token = token;
		}

		public TokenResult Result
		{
			get;
			set;
		}

		public int Token
		{
			get;
			set;
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Result = (TokenResult)reader.ReadByte ();
			this.Token = reader.ReadInt32 ();
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteByte ((byte)this.Result);
			writer.WriteInt32 (this.Token);
		}
	}

	public enum TokenResult
		: byte
	{
		/// <summary>
		/// Failed for an unknown or unsupported reason.
		/// </summary>
		FailedUnknown = 0,

		Succeeded = 1,

		/// <summary>
		/// Failed because the server requires a newer client version.
		/// </summary>
		FailedClientVersion = 2,

		/// <summary>
		/// Failed because there are too many tokens out. DOS Attack?
		/// </summary>
		FailedTokenOverflow = 3,

		/// <summary>
		/// Failed because you are banned. Failboat.
		/// </summary>
		FailedBanned = 4,
	}
}