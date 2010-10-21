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
			
			using (var g = Graphics.FromImage (self))
				g.DrawImage (imposing, new Rectangle (GetOverlayOrigin (self.Size, size, alignment), size));

			return self;
		}

		public static Image Overlay (this Image self, string imposing, Font font, ContentAlignment alignment)
		{
			using (var g = Graphics.FromImage (self))
			{
				SizeF size = g.MeasureString (imposing, font);

				Point p = GetOverlayOrigin (self.Size, new Size ((int)size.Width, (int)size.Height), alignment);
				g.DrawString (imposing, font, Brushes.Black, new PointF (p.X, p.Y));
			}

			return self;
		}

		private static Point GetOverlayOrigin (Size baseSize, Size overlaySize, ContentAlignment alignment)
		{
			int x, y;
			switch (alignment)
			{
				case ContentAlignment.BottomCenter:
					x = baseSize.Width - overlaySize.Width / 2;
					y = baseSize.Height - overlaySize.Height;
					break;

				case ContentAlignment.BottomRight:
					x = baseSize.Width - overlaySize.Width;
					y = baseSize.Height - overlaySize.Height;
					break;

				case ContentAlignment.MiddleCenter:
					x = baseSize.Width - overlaySize.Width / 2;
					y = baseSize.Height - overlaySize.Height / 2;
					break;

				default:
					throw new NotImplementedException();
			}

			return new Point (x, y);
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