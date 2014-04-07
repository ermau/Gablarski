using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Input;
using Gablarski.Client;

namespace Gablarski.Clients.ViewModels
{
	public sealed class RegisterViewModel
		: ViewModelBase, INotifyDataErrorInfo
	{
		public RegisterViewModel (IGablarskiClientContext client)
		{
			if (client == null)
				throw new ArgumentNullException ("client");

			this.client = client;
			this.registerCommand = new RelayCommand (Register, CanRegister);
		}

		public event EventHandler<ReceivedRegisterResultEventArgs> RegisterResultReceived;

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged
		{
			add { this.errors.ErrorsChanged += value; }
			remove { this.errors.ErrorsChanged -= value; }
		}

		public ICommand RegisterCommand
		{
			get { return this.registerCommand; }
		}

		public bool IsWaiting
		{
			get { return this.isWaiting; }
			private set
			{
				if (this.isWaiting == value)
					return;

				this.isWaiting = value;
				OnPropertyChanged();
			}
		}

		public string Username
		{
			get { return this.username; }
			set
			{
				if (this.username == value)
					return;

				this.username = value;

				this.errors.ClearErrors ("Username");

				if (String.IsNullOrWhiteSpace (value))
					this.errors.AddError ("Username", UsernameBlankError);

				OnPropertyChanged();
			}
		}

		public string Password
		{
			get { return this.password; }
			set
			{
				if (this.password == value)
					return;

				this.password = value;

				this.errors.ClearErrors ("Password");

				if (String.IsNullOrWhiteSpace (value))
					this.errors.AddError ("Password", PasswordBlankError);

				OnPropertyChanged();
			}
		}

		public string Password2
		{
			get { return this.password2; }
			set
			{
				if (this.password2 == value)
					return;

				this.password2 = value;

				this.errors.ClearErrors ("Password2");

				if (String.IsNullOrWhiteSpace (value))
					this.errors.AddError ("Password2", PasswordBlankError);
				if (value != Password)
					this.errors.AddError ("Password2", PasswordsDoNotMatchError);

				OnPropertyChanged();
			}
		}

		public bool HasErrors
		{
			get { return this.errors.HasErrors; }
		}

		public IEnumerable GetErrors (string propertyName)
		{
			return this.errors.GetErrors (propertyName);
		}

		private const string PasswordBlankError = "Password can not be blank";
		private const string PasswordsDoNotMatchError = "Passwords do not match";
		private const string UsernameBlankError = "Username can not be blank";
		private const string UsernameInUse = "Username is already in use";
		private const string UsernameInvalid = "Username is invalid";
		private const string PasswordIsInvalid = "Password is invalid";

		private readonly RelayCommand registerCommand;
		private readonly ErrorManager errors = new ErrorManager();
		private readonly IGablarskiClientContext client;

		private string username, password, password2;
		private bool isWaiting;

		private void OnRegisterResultReceived (ReceivedRegisterResultEventArgs e)
		{
			EventHandler<ReceivedRegisterResultEventArgs> handler = this.RegisterResultReceived;
			if (handler != null)
				handler (this, e);
		}

		private bool CanRegister()
		{
			return !HasErrors;
		}

		private async void Register()
		{
			IsWaiting = true;
			RegisterResult result = await this.client.CurrentUser.RegisterAsync (Username, Password);
			switch (result) {
				case RegisterResult.FailedPassword:
					this.errors.AddError ("Password", PasswordIsInvalid);
					break;

				case RegisterResult.FailedUsername:
					this.errors.AddError ("Username", UsernameInvalid);
					break;

				case RegisterResult.FailedUsernameInUse:
					this.errors.AddError ("Username", UsernameInUse);
					break;
			}

			OnRegisterResultReceived (new ReceivedRegisterResultEventArgs (result));
			IsWaiting = false;
		}
	}
}
