using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Gablarski.Clients.Windows
{
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