// Copyright (c) 2011, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Tempest;

namespace Gablarski.Messages
{
	public class SourceResultMessage
		: GablarskiMessage
	{
		public SourceResultMessage ()
			: base (GablarskiMessageType.SourceResult)
		{
		}

		public SourceResultMessage (string sourceName, SourceResult result, AudioSource source)
			: this ()
		{
			if (sourceName == null)
				throw new ArgumentNullException ("sourceName");

			this.SourceName = sourceName;
			this.SourceResult = result;
			this.Source = source;

			if (source == null)
			{
				switch (result)
				{
					case Messages.SourceResult.NewSource:
					case Messages.SourceResult.SourceRemoved:
					case Messages.SourceResult.Succeeded:
						throw new ArgumentNullException ("source", "source can not be null if the result didn't fail");
				}
			}
		}

		public string SourceName
		{
			get;
			set;
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

		public override void WritePayload (ISerializationContext context, IValueWriter writer)
		{
			writer.WriteString (this.SourceName);
			writer.WriteByte ((byte)this.SourceResult);

			if (this.Source != null)
				this.Source.Serialize (context, writer);
		}

		public override void ReadPayload (ISerializationContext context, IValueReader reader)
		{
			this.SourceName = reader.ReadString ();
			this.SourceResult = (SourceResult)reader.ReadByte ();

			switch (this.SourceResult)
			{
				case Messages.SourceResult.NewSource:
				case Messages.SourceResult.SourceRemoved:
				case Messages.SourceResult.Succeeded:
					this.Source = new AudioSource (context, reader);
					break;
			}
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
		//FailedDisallowedType = 5,

		/// <summary>
		/// Failed because the server does not support the requested type.
		/// </summary>
		//FailedNotSupportedType = 6,

		/// <summary>
		/// Failed because you're only permitted a single source of this type.
		/// </summary>
		//FailedPermittedSingleSourceOfType = 7,

		/// <summary>
		/// Failed because invalid arguments were supplied for the request.
		/// </summary>
		FailedInvalidArguments = 9,
		
		/// <summary>
		/// Failed because the requesting user already has a source with this name.
		/// </summary>
		FailedDuplicateSourceName = 10
	}
}