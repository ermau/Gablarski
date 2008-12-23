using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public interface IAudioEncoder
	{
		byte[] Encode (byte[] bytes);
	}
}