using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class SourceMessage
		: ServerMessage
	{
		public SourceMessage ()
			: base (ServerMessageType.Source)
		{
		}

		public SourceMessage (SourceResult SourceResult)
			: this (ServerMessageType.Source)
		{

		}

		public SourceResult SourceResult
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
	}
}