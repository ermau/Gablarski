using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Gablarski.Clients.Windows
{
	/// <summary>
	/// Interaction logic for UserView.xaml
	/// </summary>
	public partial class UserView : UserControl
	{
		public UserView()
		{
			InitializeComponent();
		}

		private void OnClickNormalVolume (object sender, EventArgs routedEventArgs)
		{
			this.volume.Value = 1;
		}

		private void OnClickedNoVolume (object sender, EventArgs e)
		{
			this.volume.Value = 0;
		}

		private void OnClickMaxVolume (object sender, EventArgs e)
		{
			this.volume.Value = this.volume.Maximum;
		}
	}
}
