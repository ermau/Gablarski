using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Sources;

namespace Gablarski.Messages
{
	public class SourceResultMessage
		: ServerMessage
	{
		public SourceResultMessage ()
			: base (ServerMessageType.SourceResult)
		{
		}

		public SourceResultMessage (SourceResult result, AudioSource source)
			: this ()
		{
			this.SourceResult = result;
			this.Source = source;
		}

		public SourceResult SourceResult
		{
			get;
			set;
		}

		public AudioSource Source
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteByte ((byte)this.SourceResult);
			this.Source.Serialize (writer);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.SourceResult = (SourceResult)reader.ReadByte ();
			this.Source = new AudioSource (reader);
		}
	}

	public enum SourceResult
		: byte
	{
		/// <summary>
		/// Another users new source.
		/// </summary>
		NewSource = 0,

		/// <summary>
		/// The source was removed.
		/// </summary>
		SourceRemoved = 8,

		/// <summary>
		/// The source was successfully requested.
		/// </summary>
		Succeeded = 1,

		/// <summary>
		/// Failed for an unknown reason.
		/// </summary>
		FailedUnknown = 2,

		/// <summary>
		/// Failed because you or the server is at it's source limit.
		/// </summary>
		FailedLimit = 3,

		/// <summary>
		/// Failed because you do not have sufficent permissions.
		/// </summary>
		FailedPermissions = 4,

		/// <summary>
		/// Failed because the MediaType requested is not allowed.
		/// </summary>
		FailedDisallowedType = 5,

		/// <summary>
		/// Failed because the server does not support the requested type.
		/// </summary>
		FailedNotSupportedType = 6,

		/// <summary>
		/// Failed because you're only permitted a single source of this type.
		/// </summary>
		FailedPermittedSingleSourceOfType = 7,
	}
}