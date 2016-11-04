using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal interface IBufferedSource
	{
		float[] ReadNextBatch();
		int SampleRate { get; }
	}
}
