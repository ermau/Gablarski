using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Gablarski.Clients.Messages;
using Gablarski.Clients.ViewModels;

namespace Gablarski.Clients.Windows
{
	/// <summary>
	/// Interaction logic for GroupView.xaml
	/// </summary>
	public partial class GroupView : UserControl
	{
		public GroupView()
		{
			InitializeComponent();

			Messenger.Register<JoinVoiceMessage> (OnJoinVoiceMessage);
		}

		private GroupViewModel ViewModel
		{
			get { return (GroupViewModel)DataContext; }
		}

		private void OnTextKeyDown (object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;

			string message = this.text.Text.Trim();
			if (message == String.Empty)
				return;

			ViewModel.SendTextMessage.Execute (
				new TextMessage (ViewModel.Group, Program.SocialClient.Persona, this.text.Text));
		}

		private void OnJoinVoiceMessage (JoinVoiceMessage msg)
		{
		}
	}
}
