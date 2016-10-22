using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Osc : Source
	{
		public Osc()
		{
			Frequency = 100;
			Amplitude = 0.25f;
		}

		public Osc(Source frequency, Source amplitude)
		{
			Frequency = frequency;
			Amplitude = amplitude;
		}

		public Osc(Source frequency, Source amplitude, string id) : base(id)
		{
			Frequency = frequency;
			Amplitude = amplitude;
		}

		public Source Frequency { get; set; }
		public Source Amplitude { get; set; }

		private class State
		{
			internal long Sample { get; set; }
			internal double Phase { get; set; }
		}

		public override Sample Play(IContext context)
		{
			var state = GetOrCreate<State>(context);

			int sampleRate = 44100;

			float amplitude = Amplitude.Play(context).Value;
			float frequency = Frequency.Play(context).Value;

			if (frequency < 0)
				frequency = 0;

			if(frequency > sampleRate / 2)
				frequency = sampleRate / 2;

			var delta = context.Sample - state.Sample;
			state.Sample = context.Sample;
			state.Phase += (frequency / sampleRate) * delta;

			while (state.Phase > 1)
				state.Phase -= 1;

			return new Sample { Value = (float)(amplitude * Math.Sin(2 * Math.PI * state.Phase)) };
		}
	}
}
