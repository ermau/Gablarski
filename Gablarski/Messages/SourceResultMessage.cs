using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class SourceResultMessage
		: ServerMessage
	{
		public SourceResultMessage ()
			: base (ServerMessageType.SourceResult)
		{
		}

		public SourceResultMessage (SourceResult result)
			: this ()
		{
			this.SourceResult = result;
		}

		public SourceResult SourceResult
		{
			get;
			set;
		}

		public uint SourceID
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			throw new NotImplementedException ();
		}

		public override void ReadPayload (IValueReader reader)
		{
			throw new NotImplementedException ();
		}
	}

	public enum SourceResult
		: byte
	{
		/// <summary>
		/// The source was successfully requested.
		/// </summary>
		Succeeded = 1,

		/// <summary>
		/// Failed because you or the server is at it's source limit.
		/// </summary>
		FailedLimit = 2,

		/// <summary>
		/// Failed because you do not have sufficent permissions.
		/// </summary>
		FailedPermissions = 3,

		/// <summary>
		/// Failed because the MediaType requested is not allowed.
		/// </summary>
		FailedDisallowedType = 4,

		/// <summary>
		/// Failed because the server does not support the requested type.
		/// </summary>
		FailedNotSupportedType = 5,
	}
}