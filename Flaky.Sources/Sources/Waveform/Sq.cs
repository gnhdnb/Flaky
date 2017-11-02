using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Sq : Source, IPipingSource
	{
		internal Sq() : this(1) { }

		internal Sq(Source amplitude) : this(amplitude, 0) { }

		internal Sq(Source amplitude, Source pwm)
		{
			Amplitude = amplitude;
			PWM = pwm;
		}

		internal Sq(Source amplitude, Source pwm, string id) : base(id)
		{
			Amplitude = amplitude;
			PWM = pwm;
		}

		private Source Frequency;
		private Source Amplitude;
		private Source PWM;
		private State state;

		private class State
		{
			internal long sample;
			internal long position;
		}

		protected override Sample NextSample(IContext context)
		{
			int sampleRate = context.SampleRate;

			float amplitude = Amplitude.Play(context).Value;
			float frequency = Frequency.Play(context).Value;
			float pwm = PWM.Play(context).Value;

			if (pwm > 1)
				pwm = 1;

			if (pwm < -1)
				pwm = -1;

			if (frequency < 0)
				frequency = 0;

			if (frequency == 0)
				return 0;

			if(frequency > sampleRate / 2)
				frequency = sampleRate / 2;

			var delta = context.Sample - state.sample;
			state.sample = context.Sample;
			state.position += delta;

			var period = sampleRate / frequency;

			float result = 0;

			result = -amplitude;

			if (state.position > period * (0.5 + pwm / 2))
			{
				result = amplitude;
			}

			if (state.position >= period)
			{
				state.position = 0;
				result = -amplitude;
			}

			return new Sample
			{
				Left = result,
				Right = result
			};
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			Initialize(context, Frequency, Amplitude, PWM);
		}

		public override void Dispose()
		{
			Dispose(Frequency, Amplitude, PWM);
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.Frequency = mainSource;
		}
	}
}
