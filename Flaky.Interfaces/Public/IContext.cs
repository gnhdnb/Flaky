using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public interface IContext
	{
		long Sample { get; }
		int SampleRate { get; }
		int Beat { get; }
		int BPM { get; }
	}
}
