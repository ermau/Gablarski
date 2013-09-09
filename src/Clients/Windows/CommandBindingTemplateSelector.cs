using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Cadenza;

namespace Gablarski.Clients.Windows
{
	public class CommandBindingTemplateSelector
		: DataTemplateSelector
	{
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
			else
				return ExistingTemplate;
		}
	}
}