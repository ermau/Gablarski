using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Codecs;

namespace Gablarski.Media.Sources
{
	public abstract class MediaSourceBase
	{
		protected MediaSourceBase (MediaType type, int sourceId, object ownerId)
		{
			this.Type = type;
			this.Id = sourceId;
			this.OwnerId = ownerId;
		}

		/// <summary>
		/// Gets the ID of the source.
		/// </summary>
		public int Id
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Gets the type of media source.
		/// </summary>
		public MediaType Type
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the owner's identifier.
		/// </summary>
		public object OwnerId
		{
			get;
			private set;
		}
		
		public abstract IAudioCodec AudioCodec { get; }
		public abstract IMediaCodec VideoCodec { get; }
	}
}