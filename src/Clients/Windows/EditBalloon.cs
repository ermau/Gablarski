using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Gablarski.Clients.Windows
{
	public enum TooltipIcon
	{
		None = 0,
		Info = 1,
		Warning = 2,
		Error = 3
	}

	public class EditBalloon 
	{
		public EditBalloon (Control parent)
		{
			if (parent == null)
				throw new ArgumentNullException ("parent");

			Parent = parent;
			Title = String.Empty;
			Icon = TooltipIcon.None;
			Text = String.Empty;
		}

		/// <summary>
		/// Show a balloon tooltip for edit control.
		/// </summary>
		public void Show()
		{
			EDITBALLOONTIP ebt = new EDITBALLOONTIP();

			ebt.pszText = Text;
			ebt.pszTitle = Title;
			ebt.ttiIcon = (int)Icon;
			ebt.cbStruct = Marshal.SizeOf(ebt);

			SendMessage (Parent.Handle, EM_SHOWBALLOONTIP, 0, ref ebt);
		}

		/// <summary>
		/// Gets or sets the Title.
		/// </summary>
		public string Title
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the display icon.
		/// </summary>
		public TooltipIcon Icon
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the display text.
		/// </summary>
		public string Text
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the parent.
		/// </summary>
		public Control Parent
		{
			get;
			private set;
		}

		private const int ECM_FIRST = 0x1500;
		private const int EM_SHOWBALLOONTIP = ECM_FIRST + 3;

		[DllImport ("User32", SetLastError=true)]
		private static extern int SendMessage (IntPtr hWnd, uint msg, int wParam, ref EDITBALLOONTIP lParam);

		[StructLayout (LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		private struct EDITBALLOONTIP
		{
			public int cbStruct;
			[MarshalAs (UnmanagedType.LPWStr)] public string pszTitle;
			[MarshalAs (UnmanagedType.LPWStr)] public string pszText;
			public int ttiIcon;
		}
	}
}