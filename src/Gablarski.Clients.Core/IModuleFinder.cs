using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gablarski.Clients
{
	public interface IModuleFinder
	{
		/// <summary>
		/// Asynchronosly loads the <see cref="Type"/>s for exported implementations of <typeparamref name="TContract"/>.
		/// </summary>
		/// <typeparam name="TContract">The type of the contract to load exported implementers of.</typeparam>
		/// <returns>A task of a collection of types that implement <typeparamref name="TContract"/>.</returns>
		Task<IReadOnlyCollection<Type>> LoadExportsAsync<TContract>();
	}
}