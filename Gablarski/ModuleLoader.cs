using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Gablarski
{
	[Flags]
	public enum ModuleLoaderOptions
	{
		/// <summary>
		/// Searches the assembly that <see cref="ModuleLoader<T>"/> is defined in.
		/// </summary>
		SearchCurrent = 1,

		/// <summary>
		/// Search the currently executing assembly for implementing types.
		/// </summary>
		SearchExecuting = 2,

		/// <summary>
		/// Search the paths supplied recursively.
		/// </summary>
		SearchRecursively = 4,
	}

	/// <summary>
	/// Finds and loads implementing types of a contract.
	/// </summary>
	/// <typeparam name="T">The contract to find implementers for.</typeparam>
	public class ModuleLoader<T>
	{
		public ModuleLoader ()
		{
			Type t = typeof (T);
			if (!t.IsInterface && !t.IsAbstract)
				throw new ArgumentException ("Must be a contract (interface or abstract class)", "T");

			this.Options = ModuleLoaderOptions.SearchCurrent | ModuleLoaderOptions.SearchExecuting;
		}

		public ModuleLoader (ModuleLoaderOptions options, IEnumerable<string> paths)
			: this()
		{
			this.Options = options;
		}

		public ModuleLoader (ModuleLoaderOptions options, params string[] paths)
			: this (options, (IEnumerable<string>)paths)
		{
		}

		public ModuleLoaderOptions Options
		{
			get;
			private set;
		}

		public IEnumerable<Type> GetImplementers ()
		{
			throw new NotImplementedException ();
		}

		private readonly IEnumerable<string> paths;

		// <Contract, Implementers>
		private static ILookup<Type, Type> SearchAssembly (Assembly asm, IEnumerable<Type> contracts)
		{
			MutableLookup<Type, Type> lookup = new MutableLookup<Type, Type> ();

			foreach (var type in asm.GetTypes ().Where (t => t.IsPublic))
			{
				foreach (var c in contracts)
				{
					if (!type.IsAssignableFrom (c))
						continue;

					lookup.Add (c, type);
				}
			}

			return lookup;
		}
	}
}