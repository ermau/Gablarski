using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Gablarski.Clients.Windows
{
	public static class Extensions
	{
		public static Icon ToIcon (this Bitmap self)
		{
			return Icon.FromHandle (self.GetHicon());
		}

		public static string ToDisplayString (this Exception self)
		{
			StringBuilder builder = new StringBuilder(self.GetType().Name);
			builder.Append (": ");
			builder.Append (self.Message);

			DisplayRecurseInnerExceptions (self.InnerException, builder);

			return builder.ToString();
		}

		private static void DisplayRecurseInnerExceptions (Exception ex, StringBuilder builder)
		{
			if (ex == null)
				return;

			builder.Append (Environment.NewLine);
			builder.Append ("[Inner] ");
			builder.Append (ex.GetType().Name);
			builder.Append (": ");
			builder.Append (ex.Message);

			DisplayRecurseInnerExceptions (ex.InnerException, builder);
		}
	}
}