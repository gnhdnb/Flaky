using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public interface IWaveReaderFactory
	{
		IWaveReader Create(IContext context, string fileName);

		IMultipleWaveReader Create(IContext context, string folder, string pack);
	}
}
