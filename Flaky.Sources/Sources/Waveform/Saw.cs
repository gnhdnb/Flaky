using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Saw : Source
	{
		public Saw()
		{
			Frequency = 100;
			Amplitude = 0.25f;
		}

		public Saw(Source frequency, Source amplitude)
		{
			Frequency = frequency;
			Amplitude = amplitude;
		}

		public Saw(Source frequency, Source amplitude, string id) : base(id)
		{
			Frequency = frequency;
			Amplitude = amplitude;
		}

		private Source Frequency;
		private Source Amplitude;
		private State state;

		private class State
		{
			internal long sample;
			internal float position;
		}

		public override Sample Play(IContext context)
		{
			int sampleRate = context.SampleRate;

			float amplitude = Amplitude.Play(context).Value;
			float frequency = Frequency.Play(context).Value;

			if (frequency < 0)
				frequency = 0;

			if(frequency > sampleRate / 2)
				frequency = sampleRate / 2;

			var delta = context.Sample - state.sample;
			state.sample = context.Sample;

			if (state.position < -Math.Abs(amplitude)
				|| state.position > Math.Abs(amplitude))
				state.position = amplitude;

			state.position -= 2 * amplitude * frequency / (float)sampleRate;

			return new Sample
			{
				Left = state.position,
				Right = state.position
			};
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			Initialize(context, Frequency, Amplitude);
		}

        public override void Dispose()
        {
            Dispose(Frequency, Amplitude);
        }
    }
}
