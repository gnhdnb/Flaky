using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public interface IWaveReader
	{
		Vector2? Read(long index);
		Vector2? Read(float index);
		long Length { get; }
	}
}
