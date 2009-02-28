using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class Channel
	{
		/// <summary>
		/// Gets the name of the channel.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the description of the channel.
		/// </summary>
		string Description { get; }
		
		/// <summary>
		/// Gets the player limit. 0 for no limit.
		/// </summary>
		int PlayerLimit { get; }
	}
}