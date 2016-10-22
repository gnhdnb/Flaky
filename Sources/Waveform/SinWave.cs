using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Sin : Source
	{
		public Sin()
		{
			Frequency = 100;
			Amplitude = 0.25f;
		}

		public Sin(Source frequency, Source amplitude)
		{
			Frequency = frequency;
			Amplitude = amplitude;
		}

		public Source Frequency { get; set; }
		public Source Amplitude { get; set; }

		private long latestSample;
		private double phase;

		public override Sample Play(IContext context)
		{
			int sampleRate = 44100;

			float amplitude = Amplitude.Play(context).Value;
			float frequency = Frequency.Play(context).Value;

			var delta = context.Sample - latestSample;
			latestSample = context.Sample;
			phase += (frequency / sampleRate) * delta;

			while (phase > 1)
				phase -= 1;

			return new Sample { Value = (float)(amplitude * Math.Sin(2 * Math.PI * phase)) };
		}
	}
}
