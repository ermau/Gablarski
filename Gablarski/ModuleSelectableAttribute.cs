using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	[AttributeUsage (AttributeTargets.Class)]
	public class ModuleSelectableAttribute
		: Attribute
	{
		public ModuleSelectableAttribute()
			: this (true)
		{
		}

		public ModuleSelectableAttribute (bool selectable)
		{
			this.Selectable = selectable;
		}

		public bool Selectable { get; set; }
	}
}