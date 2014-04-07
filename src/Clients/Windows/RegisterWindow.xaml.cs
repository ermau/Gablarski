using Gablarski.Clients.ViewModels;

namespace Gablarski.Clients.Windows
{
	/// <summary>
	/// Interaction logic for RegisterWindow.xaml
	/// </summary>
	public partial class RegisterWindow
	{
		public RegisterWindow()
		{
			InitializeComponent();
			DataContext = new RegisterViewModel (null);
		}
	}
}
