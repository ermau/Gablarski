using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Kennedy.ManagedHooks;

namespace Gablarski.Client
{
	public static class PushToTalk
	{
		static PushToTalk()
		{
			hook.KeyboardEvent += hook_KeyboardEvent;
			hook.InstallHook();
		}

		static void hook_KeyboardEvent (KeyboardEvents kEvent, Keys key)
		{
			if (key != Keys)
				return;

			if (kEvent == KeyboardEvents.KeyDown)
			{
				talking = true;
				OnTalking();
			}
			else if (kEvent == KeyboardEvents.KeyUp)
			{
				talking = false;
				OnStoppedTalking();
			}
		}

		public static void Uninstall()
		{
			hook.UninstallHook();
		}

		public static event EventHandler Talking;
		public static event EventHandler StoppedTalking;

		public static Keys Keys
		{
			get
			{
				lock (lck)
				{
					return keys;
				}
			}

			set
			{
				lock (lck)
				{
					keys = value;
				}
			}
		}

		private static KeyboardHook hook = new KeyboardHook();
		private static bool talking;
		private static Keys keys;
		private static object lck = new object();

		private static void OnTalking()
		{
			var talking = Talking;
			if (talking != null)
				talking (null, EventArgs.Empty);
		}

		private static void OnStoppedTalking()
		{
			var stalking = StoppedTalking;
			if (stalking != null)
				stalking (null, EventArgs.Empty);
		}
	}
}