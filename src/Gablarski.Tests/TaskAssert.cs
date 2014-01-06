using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Gablarski.Tests
{
	public static class TaskAssert
	{
		public static void Threw<T> (Task task)
			where T : Exception
		{
			if (task == null)
				throw new ArgumentNullException ("task");

			try {
				task.Wait();
				Assert.Fail ("Task did not throw an exception");
			} catch (AggregateException aex) {
				aex = aex.Flatten();
				Assert.IsTrue (aex.InnerExceptions.OfType<T>().Any(), "Task did not throw a " + typeof (T).Name);
			}
		}
	}
}