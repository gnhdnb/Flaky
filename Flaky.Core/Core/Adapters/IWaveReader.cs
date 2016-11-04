using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public interface IWaveReader
	{
		float? Read(long index);
	}
}
