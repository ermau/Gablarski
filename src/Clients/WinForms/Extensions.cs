using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
			builder.AppendLine ();
			builder.AppendLine (self.StackTrace);

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

		public static DialogResult ShowDialogOnFormThread (this Form self, Form formForThread)
		{
			return (DialogResult)formForThread.Invoke ((Func<IWin32Window, DialogResult>)self.ShowDialog, formForThread);
		}

		public static DbCommand CreateCommand (this DbConnection self, string text)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
			if (text == null)
				throw new ArgumentNullException ("text");

			var cmd = self.CreateCommand();
			cmd.CommandText = text;
			return cmd;
		}
	}
}