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
		DateTime Timestamp { get; }
		int SampleRate { get; }
		int Beat { get; }
		bool MetronomeTick { get; }
		int BPM { get; }
	}
}
