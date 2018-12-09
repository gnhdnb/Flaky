using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal interface IWaveReader
	{
		Vector2? Read(long index);
		long Length { get; }
	}
}
