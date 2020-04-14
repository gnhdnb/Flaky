using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flaky
{
	public interface IAudioStreamReader
	{
		float[] ReadVorbis(Stream stream);
		float[] ReadWav(Stream stream);
	}
}
