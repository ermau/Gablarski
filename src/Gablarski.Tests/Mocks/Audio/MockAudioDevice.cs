using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;

namespace Gablarski.Tests.Mocks.Audio
{
	public class MockAudioDevice
		: IAudioDevice
	{
		public MockAudioDevice (string name)
		{
			this.Name = name;
		}

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
		}

		#endregion

		#region Implementation of IAudioDevice

		public string Name
		{
			get; private set;
		}

		#endregion

		public override int GetHashCode ()
		{
			return this.Name.GetHashCode();
		}
	}
}