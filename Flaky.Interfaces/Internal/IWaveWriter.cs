using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	interface IWaveWriter : IDisposable
	{
		void Write(Sample sample);
	}
}
