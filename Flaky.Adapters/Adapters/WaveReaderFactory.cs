using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class WaveReaderFactory : IWaveReaderFactory
	{
		public IWaveReader Create(string fileName)
		{
			return new WaveReader(fileName);
		}
	}
}
