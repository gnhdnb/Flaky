using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	interface IMultipleWaveReader
	{
		Vector2? Read(int wave, long index);
		long Length(int wave);
		int Waves { get; }
	}
}
