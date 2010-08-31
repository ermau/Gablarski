using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Gablarski.Clients.Windows.Entities;

namespace Gablarski.Clients.Windows
{
	public class CommandBindingTemplateSelector
		: DataTemplateSelector
	{
		public DataTemplate NewTemplate
		{
			get;
			set;
		}

		public DataTemplate RecordingTemplate
		{
			get;
			set;
		}

		public DataTemplate ExistingTemplate
		{
			get;
			set;
		}

		public override DataTemplate SelectTemplate (object item, DependencyObject container)
		{
			var binding = (item as CommandBindingSettingEntry);
			if (binding == null)
				return base.SelectTemplate (item, container);

			if (binding.Recording)
				return RecordingTemplate;
			else if (binding.Id != 0)
				return ExistingTemplate;
			else
				return NewTemplate;
		}
	}
}