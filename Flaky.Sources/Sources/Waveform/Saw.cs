using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Saw : Source, IPipingSource
	{
		internal Saw()
		{
			Amplitude = 1;
		}

		internal Saw(Source amplitude)
		{
			Amplitude = amplitude;
		}

		internal Saw(Source amplitude, string id) : base(id)
		{
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

		protected override Vector2 NextSample(IContext context)
		{
			int sampleRate = context.SampleRate;

			float amplitude = Amplitude.Play(context).X;
			float frequency = Frequency.Play(context).X;

			if (frequency < 0)
				frequency = 0;

			if (frequency == 0)
				return new Vector2(0, 0);

			if(frequency > sampleRate / 2)
				frequency = sampleRate / 2;

			var delta = context.Sample - state.sample;
			state.sample = context.Sample;

			if (state.position < -Math.Abs(amplitude)
				|| state.position > Math.Abs(amplitude))
				state.position = amplitude;

			state.position -= 2 * amplitude * frequency / (float)sampleRate;

			return new Vector2(state.position, state.position);
		}

		protected override void Initialize(IContext context)
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
			this.Frequency = mainSource;
		}
	}
}
