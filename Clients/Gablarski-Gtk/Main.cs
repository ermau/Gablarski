using System;
using Gtk;

namespace GablarskiGtk
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			
			ConnectWindow connect = new ConnectWindow();
			connect.Show ();
			
			Application.Run ();
		}
	}
}
