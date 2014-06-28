using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Gablarski.Clients.Windows
{
	public class BoolContentSelector
		: DataTemplateSelector
	{
		public string TrueContentKey
		{
			get { return this.trueContentKey; }
			set
			{
				this.trueContentKey = value;
				this.trueTemplate = (DataTemplate)XamlReader.Parse (String.Format (Template, value));
			}
		}

		public string FalseContentKey
		{
			get { return this.falseContentKey; }
			set
			{
				this.falseContentKey = value;
				this.falseTemplate = (DataTemplate) XamlReader.Parse (String.Format (Template, value));
			}
		}

		public override DataTemplate SelectTemplate (object item, DependencyObject container)
		{
			return ((bool) item) ? this.trueTemplate : this.falseTemplate;
		}

		private DataTemplate falseTemplate;
		private string falseContentKey;

		private DataTemplate trueTemplate;
		private string trueContentKey;

		private const string Template = "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><ContentPresenter Content=\"{{DynamicResource {0}}}\" /></DataTemplate>";
	}

	public class BoolToContentConverter
		: DependencyObject, IValueConverter
	{
		public static readonly DependencyProperty TrueContentProperty = DependencyProperty.Register (
			"TrueContent", typeof (object), typeof (BoolToContentConverter), new PropertyMetadata (default(object)));

		public object TrueContent
		{
			get { return GetValue (TrueContentProperty); }
			set { SetValue (TrueContentProperty, value); }
		}

		public static readonly DependencyProperty FalseContentProperty = DependencyProperty.Register (
			"FalseContent", typeof (object), typeof (BoolToContentConverter), new PropertyMetadata (default(object)));

		public object FalseContent
		{
			get { return GetValue (FalseContentProperty); }
			set { SetValue (FalseContentProperty, value); }
		}

		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ((bool) value) ? TrueContent : FalseContent;
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}