using System;
using System.Configuration;

namespace Barrel
{
	public class Barrel
	{
		public static void Main (string[] args)
		{
			log4net.Config.XmlConfigurator.Configure ();
		;
			//var s = ConfigurationManager.GetSection ("servers");
			var s = ConfigurationManager.AppSettings["servers"];
			Console.WriteLine (s.ToString());
			log4net.LogManager.GetLogger ("Barrel").Debug (s);
		}
	}
}