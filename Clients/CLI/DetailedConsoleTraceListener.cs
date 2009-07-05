using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class DetailedConsoleTraceListener
		: TraceListener
	{
		#region Overrides of TraceListener

		/// <summary>
		/// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
		/// </summary>
		/// <param name="message">A message to write.</param>
		/// <filterpriority>2</filterpriority>
		public override void Write (string message)
		{
			Console.Write (message);
		}

		/// <summary>
		/// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
		/// </summary>
		/// <param name="message">A message to write.</param>
		/// <filterpriority>2</filterpriority>
		public override void WriteLine (string message)
		{
			if ((TraceOutputOptions & TraceOptions.DateTime) == TraceOptions.DateTime)
				Write (DateTime.Now + " ");

			if ((TraceOutputOptions & TraceOptions.Timestamp) == TraceOptions.Timestamp)
				Write (Stopwatch.GetTimestamp() + " ");

			Console.WriteLine (message);
		}

		#endregion
	}
}