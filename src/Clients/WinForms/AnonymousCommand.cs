using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Gablarski.Clients.Windows
{
	public class AnonymousCommand
		: ICommand
	{
		private readonly Action<object> execute;
		private readonly Func<object, bool> canExecute;

		public AnonymousCommand (Action<object> execute, Func<object, bool> canExecute)
		{
			if (execute == null)
				throw new ArgumentNullException ("execute");
			if (canExecute == null)
				throw new ArgumentNullException ("canExecute");

			this.execute = execute;
			this.canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute (object parameter)
		{
			this.execute (parameter);
		}

		public bool CanExecute (object parameter)
		{
			return this.canExecute (parameter);
		}
	}
}