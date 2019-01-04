using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Osc : Source, IPipingSource
	{
		internal Osc()
		{
			Amplitude = 1f;
		}

		internal Osc(Source amplitude)
		{
			Amplitude = amplitude;
		}

		internal Osc(Source amplitude, string id) : base(id)
		{
			Amplitude = amplitude;
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

		private Source Frequency;
		private Source Amplitude;
		private State state;

		private class State
		{
			internal long sample;
			internal double phase;
		}

		protected override Vector2 NextSample(IContext context)
		{
			int sampleRate = context.SampleRate;

			float amplitude = Amplitude.Play(context).X;
			float frequency = Frequency.Play(context).X;

			if (frequency < 0)
				frequency = 0;

			if(frequency > sampleRate / 2)
				frequency = sampleRate / 2;

			if (frequency == 0)
				return new Vector2(0, 0);

			var delta = context.Sample - state.sample;
			state.sample = context.Sample;
			state.phase += (frequency / sampleRate) * delta;

			while (state.phase > 1)
				state.phase -= 1;

			float value = (float)(amplitude * Math.Sin(2 * Math.PI * state.phase));

			return new Vector2(value, value);
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

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			Frequency = mainSource;
		}
	}
}
