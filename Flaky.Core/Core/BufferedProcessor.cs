using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class BufferedProcessor : IExternalSourceProcessor
	{
		private const int bufferSize =
			SeparateThreadProcessor.bufferSize * (SeparateThreadProcessor.readBuffersCount + 1);

		private readonly IFlakySource source;
		private readonly Sample[] buffer = new Sample[bufferSize];

		private long writeSample = -1;

		internal BufferedProcessor(IFlakySource source)
		{
			this.source = source;
		}

		public Sample Play(IContext context)
		{
			if (context.Sample > writeSample)
			{
				writeSample = context.Sample;
				var sample = source.PlayInCurrentThread(context);
				buffer[writeSample % bufferSize] = sample;
				return sample;
			}
			else
			{
				return buffer[context.Sample % bufferSize];
			}
		}
	}
}
