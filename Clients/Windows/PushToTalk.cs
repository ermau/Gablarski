using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Gablarski.Clients.Windows
{
	public enum PushToTalkSupplier
	{
		Keyboard,
		Mouse
	}

	public class PushToTalk
	{
		public PushToTalk (Keys keys)
		{
			this.keyboard = true;
			this.KeyboardKeys = keys;
		}

		//public PushToTalk (MouseButtons mouseButtons)
		//{
		//    this.MouseButtons = mouseButtons;
		//}

		public PushToTalkSupplier Supplier
		{
			get { return (keyboard) ? PushToTalkSupplier.Keyboard : PushToTalkSupplier.Mouse; }
		}

		public Keys KeyboardKeys
		{
			get; private set;
		}

		//public MouseButtons MouseButtons
		//{
		//    get; private set;
		//}

		public override string ToString ()
		{
			//return (keyboard) ? "k" + KeyboardKeys : "m" + MouseButtons;
			return "k" + KeyboardKeys;
		}

		public static PushToTalk Parse (string value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (value.Trim() == String.Empty)
				throw new ArgumentException ("Empty value.", "value");

			//if (value[0] == 'm')
			//{
			//    int ival;
			//    if (!Int32.TryParse (value.Substring (1), out ival))
			//        throw new FormatException();
			//    else
			//        return new PushToTalk ((MouseButtons)ival);
			//}
			if (value[0] == 'k')
			{
			    int ival;
			    if (!Int32.TryParse (value.Substring (1), out ival))
			        throw new FormatException();
			    else
			        return new PushToTalk ((Keys)ival);
			}
			else
				throw new FormatException();
		}

		private readonly bool keyboard;
	}
}