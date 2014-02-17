using System;
using System.Windows;
using System.Windows.Interop;
using Gablarski.Clients.ViewModels;

namespace Gablarski.Clients.Windows
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow
	{
		public SettingsWindow()
		{
			InitializeComponent();

			IntPtr windowHandle = new WindowInteropHelper (this).Handle;
			var vm = new SettingsViewModel (windowHandle);
			vm.Closing += OnClosing;
			DataContext = vm;
		}

		private void OnClosing (object sender, EventArgs eventArgs)
		{
			Close();
		}

		private void UseCurrentMusicVolumeChecked (object sender, RoutedEventArgs e)
		{
			this.normalVolume.IsEnabled = !(this.useCurrentMusicVolume.IsChecked.HasValue && this.useCurrentMusicVolume.IsChecked.Value);
		}
	}
}
