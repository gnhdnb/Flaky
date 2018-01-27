using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Tape : Source, IPipingSource
	{
		private Source source;
		private Noise noise = new Noise();
		private Osc lfo;
		private State state;

		private class State
		{
			const int preeffectBufferSize = 15;
			private double velocity = 0;
			private double velocity2 = 0;
			private double current = 0;
			private double[] preeffectBuffer = new double[preeffectBufferSize];
			private int preeffectBufferCounter = 0;
			private double preeffectAverage = 0;
			private double hpIntegrator = 0;
			private double lpIntegrator = 0;

			internal Sample[] detonationBuffer = new Sample[44100];
			internal int detonationBufferPosition;
			internal long sample;

			public Sample NextSample(Sample stereo, float noise, float lfo)
			{
				var sum = stereo.Left + stereo.Right;
				var difference = stereo.Left - stereo.Right;

				var tunedSample = Drive(sum * 8 + noise * 0.001f) * 0.25f * 0.07f;

				var currentSample = preeffectBuffer[preeffectBufferCounter];

				preeffectAverage += tunedSample / preeffectBufferSize;
				preeffectAverage -= currentSample / preeffectBufferSize;
				preeffectBuffer[preeffectBufferCounter] = tunedSample;
				preeffectBufferCounter++;
				if (preeffectBufferCounter >= preeffectBufferSize)
					preeffectBufferCounter = 0;

				var acc = (preeffectAverage - currentSample);

				velocity += acc * acc * acc * 30;
				velocity = velocity * 0.9f;

				velocity2 = velocity2 * 0.4f;
				velocity2 += (currentSample - current) * 2;

				current += velocity + velocity2;

				current = Drive(current * 20) / 20;

				hpIntegrator += (current - hpIntegrator) / (400 * (1 - Math.Abs(hpIntegrator * 20)));

				lpIntegrator += (current - hpIntegrator - lpIntegrator) /
					(1.5f + Math.Abs(lpIntegrator * 30) * Math.Abs(lpIntegrator * 30));

				var result = new Sample
				{
					Right = (float)lpIntegrator * 10 - difference / 2,
					Left = (float)lpIntegrator * 10 + difference / 2
				};

				return Detonation(result, lfo);
			}

			private Sample Detonation(Sample sample, float lfo)
			{
				detonationBufferPosition ++;

				while (detonationBufferPosition >= detonationBuffer.Length)
					detonationBufferPosition -= detonationBuffer.Length;

				detonationBuffer[detonationBufferPosition] = sample;

				var offset = (lfo - 1) * 10 - 1;

				var roughOffset = (int)Math.Floor(offset);
				var fineOffset = (float)(offset - Math.Floor(offset));

				var readPosition = detonationBufferPosition + roughOffset;

				return
					detonationBuffer[CorrectPosition(readPosition)] * (1 - fineOffset)
					+ detonationBuffer[CorrectPosition(readPosition + 1)] * fineOffset;
			}

			private long CorrectPosition(long position)
			{
				if (position < 0)
					position += detonationBuffer.Length;

				if (position >= detonationBuffer.Length)
					position -= detonationBuffer.Length;

				return position;
			}

			private double Drive(double value)
			{
				return Math.Sign(value) * (1 - (float)Math.Pow(Math.E, -Math.Abs(value)));
			}
		}

		internal Tape(string id) : base(id)
		{
			lfo = new Osc(5, 1, $"{id}_lfo");
		}

		public override void Dispose()
		{
			Dispose(noise, source,lfo);
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			Initialize(context, noise, source, lfo);
		}

		protected override Sample NextSample(IContext context)
		{
			var inputSample = source.Play(context);
			var noiseSample = noise.Play(context);
			var lfoSample = lfo.Play(context);

			return state.NextSample(
				inputSample, 
				noiseSample.Value,
				lfoSample.Value);
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}
	}
}
