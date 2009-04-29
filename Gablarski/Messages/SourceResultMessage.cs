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

		public SourceResultMessage (SourceResult result, Type mediaSourceType)
			: this ()
		{
			this.SourceResult = result;
			this.MediaSourceType = mediaSourceType;
		}

		public SourceResultMessage (SourceResult result, Type mediaSourceType, int ownerID)
			: this (result, mediaSourceType)
		{
			this.OwnerID = ownerID;
		}

		public SourceResult SourceResult
		{
			get;
			set;
		}

		public int SourceID
		{
			get;
			set;
		}

		public string MediaSourceTypeName
		{
			get; set;
		}

		public Type MediaSourceType
		{
			get { return Type.GetType (this.MediaSourceTypeName); }
			set { this.MediaSourceTypeName = value.AssemblyQualifiedName; }
		}

		public int OwnerID
		{
			get;
			set;
		}

		public IMediaSource GetSource ()
		{
			return MediaSources.Create (this.MediaSourceType, this.SourceID);
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteByte ((byte)this.SourceResult);
			writer.WriteInt32 (this.SourceID);
			writer.WriteString (this.MediaSourceTypeName);

			if (this.SourceResult == SourceResult.NewSource)
				writer.WriteInt32 (this.OwnerID);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.SourceResult = (SourceResult)reader.ReadByte ();
			this.SourceID = reader.ReadInt32 ();
			this.MediaSourceTypeName = reader.ReadString();

			if (this.SourceResult == SourceResult.NewSource)
				this.OwnerID = reader.ReadInt32 ();
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
	}
}