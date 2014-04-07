using System;
using System.Globalization;
using System.Windows.Data;

namespace Gablarski.Clients.Windows
{
	class CaseConverter
		: IValueConverter
	{
		public bool ToUpper
		{
			get;
			set;
		}

		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			string v = value as string;
			if (v == null)
				return null;

			return (ToUpper) ? v.ToUpper() : v.ToLower();
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}