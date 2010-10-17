using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gablarski.Clients.Windows.Properties;

namespace Gablarski.Clients.Windows
{
	public static class Extensions
	{
		public static Icon ToIcon (this Bitmap self)
		{
			return Icon.FromHandle (self.GetHicon());
		}

		public static Image Overlay (this Image self, Image imposing)
		{
			return Overlay (self, imposing, ContentAlignment.MiddleCenter);
		}

		public static Image Overlay (this Image self, Image imposing, ContentAlignment alignment)
		{
			return Overlay (self, imposing, imposing.Size, alignment);
		}

		public static Image Overlay (this Image self, Image imposing, Size size, ContentAlignment alignment)
		{
			if (self == null)
				throw new ArgumentNullException ("self");

			int x, y;
			switch (alignment)
			{
				case ContentAlignment.BottomCenter:
					x = self.Size.Width - size.Width / 2;
					y = self.Size.Height - size.Height;
					break;

				case ContentAlignment.BottomRight:
					x = self.Size.Width - size.Width;
					y = self.Size.Height - size.Height;
					break;

				case ContentAlignment.MiddleCenter:
					x = self.Size.Width - size.Width / 2;
					y = self.Size.Height - size.Height / 2;
					break;

				default:
					throw new NotImplementedException();
			}

			using (var g = Graphics.FromImage (self))
				g.DrawImage (imposing, new Rectangle (new Point (x, y), size));

			return self;
		}

		public static Image ToErrorIcon (this Image self)
		{
			return self.Overlay (Resources.ErrorOverlay, ContentAlignment.BottomRight);
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

		public static string GetSimpleName (this Type self)
		{
			if (self == null)
				throw new NullReferenceException();

			return String.Format ("{0}, {1}", self.FullName, self.Assembly.GetName().Name);
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