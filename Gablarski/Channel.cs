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
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the description of the channel.
		/// </summary>
		public string Description
		{
			get;
			set;
		}
		
		/// <summary>
		/// Gets the player limit. 0 for no limit.
		/// </summary>
		public int PlayerLimit
		{
			get;
			set;
		}
	}
}