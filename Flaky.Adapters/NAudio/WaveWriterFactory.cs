using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal class WaveWriterFactory : IWaveWriterFactory
	{
		public IWaveWriter Create(string fileName, int sampleRate)
		{
			return new WaveWriter(fileName, sampleRate);
		}
	}
}
