using System;

namespace Gablarski
{
	/// <summary>
	/// Media types
	/// </summary>
	[Flags]
	public enum MediaTypes
		: byte
	{
		/// <summary>
		/// Voice (Players talking)
		/// </summary>
		Voice = 1,

		/// <summary>
		/// Music
		/// </summary>
		Music = 2,

		/// <summary>
		/// All types
		/// </summary>
		All = Voice | Music
	}

	public enum MediaType
		: byte
	{
		Voice,
		Music
	}
}