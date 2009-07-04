using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Gablarski.Clients.Windows.Entities;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace Gablarski.Clients.Windows
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main ()
		{
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);

			try
			{
				var login = new LoginForm();
				if (login.ShowDialog() == DialogResult.OK)
					Application.Run (new MainForm (login.Entry));
			}
			finally
			{
				Persistance.CurrentSession.Flush();
			}
		}
	}
}