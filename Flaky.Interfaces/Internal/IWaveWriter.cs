using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	interface IWaveWriter : IDisposable
	{
		void Write(Vector2 sample);
	}
}
