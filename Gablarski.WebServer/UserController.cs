using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using HttpServer.Controllers;

namespace Gablarski.WebServer
{
	public class UserController
		: RequestController
	{
		public UserController (ConnectionManager manager)
		{
			this.manager = manager;
		}

		public string Login()
		{
			if (Request.Method != "POST")
			{
				Response.Redirect ("/login.html");
				return null;
			}

			if (!Request.Form.Contains ("username") || !Request.Form.Contains ("password"))
				return "Invalid Login";

			manager.ProcessSession (Session, Response);

			var result = manager.SendAndReceive<LoginMessage, LoginResultMessage> (
							new LoginMessage { Username = Request.Form["username"].Value, Password = Request.Form["password"].Value }, Session);

			var permissions = manager.Receive<PermissionsMessage> (Session);

			if (!result.Result.Succeeded)
				return "Invalid Login";
			else if (permissions.Permissions.CheckPermission (PermissionName.AdminPanel))
			{
				Session["loggedIn"] = true;
				manager.SaveSession (Session);
				Response.Redirect ("/admin");
				return null;
			}
			else
				return "Insufficient Permissions";
		}

		private readonly ConnectionManager manager;

		#region Overrides of RequestController

		/// <summary>
		/// Make a clone of this controller
		/// </summary>
		/// <returns>
		/// a new controller with the same base information as this one.
		/// </returns>
		public override object Clone()
		{
			return new UserController (this.manager);
		}

		#endregion
	}
}