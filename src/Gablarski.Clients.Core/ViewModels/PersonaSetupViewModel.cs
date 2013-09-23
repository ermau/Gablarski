using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;

namespace Gablarski.Clients.ViewModels
{
	public sealed class PersonaSetupViewModel
		: ViewModelBase
	{
		public PersonaSetupViewModel()
		{
			// Kick off myster-man acquisition
			AvatarCache.GetAvatarAsync (null);

			this.done = new RelayCommand(OnDone, CanFinish);
		}

		public event EventHandler SetupDone;

		private string nickname;
		public string Nickname
		{
			get { return this.nickname; }
			set
			{
				if (this.nickname == value)
					return;

				this.nickname = value;
				OnPropertyChanged();
				this.done.RaiseCanExecuteChanged();
			}
		}

		private string avatar;
		public string Avatar
		{
			get { return this.avatar; }
			set
			{
				if (this.avatar == value)
					return;

				this.avatar = value;
				OnPropertyChanged();
			}
		}

		private readonly RelayCommand done;
		public ICommand Done
		{
			get { return this.done; }
		}

		private bool CanFinish()
		{
			return !String.IsNullOrWhiteSpace (Nickname);
		}

		private void OnDone()
		{
			// If it's an email, convert it to a special gravatar uri, we don't want
			// to be sending emails to other people.
			if (!String.IsNullOrWhiteSpace (Avatar) && Avatar.Contains ("@")) {
				MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
				byte[] hashData = md5.ComputeHash (Encoding.ASCII.GetBytes (Avatar));
				Avatar = "gravatar://" + hashData.Aggregate (String.Empty, (s, b) => s + b.ToString ("x2"));
			}

			Settings.Avatar = Avatar;
			Settings.Nickname = Nickname;
			Settings.Save();

			var done = SetupDone;
			if (done != null)
				done (this, EventArgs.Empty);
		}
	}
}
