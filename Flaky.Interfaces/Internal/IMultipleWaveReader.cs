using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	interface IMultipleWaveReader
	{
		Sample? Read(int wave, long index);
		long Length(int wave);
		int Waves { get; }
	}
}
