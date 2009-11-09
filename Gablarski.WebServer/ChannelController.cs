using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using HttpServer.Controllers;

namespace Gablarski.WebServer
{
	public class ChannelController
		: RequestController
	{
		private readonly ConnectionManager manager;

		public ChannelController (ConnectionManager manager)
		{
			this.manager = manager;
		}

		[BeforeFilter]
		public bool ValidateUser()
		{
			if (!manager.ProcessSession (Session, Response))
			{
				Response.Redirect ("/login");
				return false;
			}

			return true;
		}

		#region Overrides of RequestController

		/// <summary>
		/// Make a clone of this controller
		/// </summary>
		/// <returns>
		/// a new controller with the same base information as this one.
		/// </returns>
		public override object Clone()
		{
			return new ChannelController (this.manager);
		}

		#endregion
	}
}