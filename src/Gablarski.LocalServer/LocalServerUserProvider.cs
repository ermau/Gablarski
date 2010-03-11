
using System;
using Gablarski.Server;
using NHibernate;
using NHibernate.Linq;

namespace Gablarski.LocalServer
{
	public class LocalServerUserProvider
		: IUserProvider
	{
		public bool UpdateSupported
		{
			get { return true; }
		}
		
		public UserRegistrationMode RegistrationMode
		{
			get { return UserRegistrationMode.Normal; }
		}
		
		public string RegistrationContent
		{
			get { return String.Empty; }
		}
		
		public IEnumerable<User> GetUsers()
		{
			using (ISession s = Persistance.SessionFactory.OpenSession())
			{
				return s.Linq<LocalUser>().Cast<User>().ToList();
			}
		}
		
		public bool UserExists (string username)
		{
			if (username == null)
				throw new ArgumentNullException ("username");
			
			username = username.Trim().ToLower();
			
			using (ISession s = Persistance.SessionFactory.OpenSession())
			{
				return s.Linq<LocalUser>().Any (u => u.UserName.Trim().ToLower() == username);
			}
		}
		
		
	}
}
