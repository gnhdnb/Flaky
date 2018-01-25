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
			const int queueSize = 15;
			private double velocity = 0;
			private double velocity2 = 0;
			private double current = 0;
			private double oscillator = 0;
			private double[] queue = new double[queueSize];
			private int queueCounter = 0;
			private double latestSample = 0;
			private double queueAverage = 0;
			private double hpIntegrator = 0;
			private double lpIntegrator = 0;

			internal float[] buffer = new float[44100];
			internal int position;
			internal long sample;

			public float NextSample(float sample, float noise, float lfo)
			{
				var tunedSample = OD(sample * 8 + noise * 0.001f) * 0.25f * 0.07f;

				var currentSample = queue[queueCounter];

				queueAverage += tunedSample / queueSize;
				queueAverage -= currentSample / queueSize;
				queue[queueCounter] = tunedSample;
				queueCounter++;
				if (queueCounter >= queueSize)
					queueCounter = 0;

				var acc = (queueAverage - currentSample);

				velocity += acc * acc * acc * 30;
				velocity = velocity * 0.9f;

				velocity2 = velocity2 * 0.4f;
				velocity2 += (currentSample - current) * 2;

				current += velocity + velocity2;

				current = OD(current * 20) / 20;

				latestSample = currentSample;

				hpIntegrator += (current - hpIntegrator) / (400 * (1 - Math.Abs(hpIntegrator * 20)));

				lpIntegrator += (current - hpIntegrator - lpIntegrator) /
					(1.5f + Math.Abs(lpIntegrator * 30) * Math.Abs(lpIntegrator * 30));

				return Detonation((float)lpIntegrator * 20, lfo);
			}

			private float Detonation(float sample, float lfo)
			{
				position ++;

				while (position >= buffer.Length)
					position -= buffer.Length;

				buffer[position] = sample;

				var finetune = (lfo - 1) * 10 - 1;

				var roughPoint = (int)Math.Floor(finetune);
				var fineOffset = (float)(finetune - Math.Floor(finetune));

				var readPosition = position + roughPoint;

				return
					buffer[CorrectPosition(readPosition)] * (1 - fineOffset)
					+ buffer[CorrectPosition(readPosition + 1)] * fineOffset;
			}

			private long CorrectPosition(long position)
			{
				if (position < 0)
					position += buffer.Length;

				if (position >= buffer.Length)
					position -= buffer.Length;

				return position;
			}

			private double OD(double value)
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
			var result = state.NextSample(
				inputSample.Value, 
				noiseSample.Value,
				lfoSample.Value);

			return new Sample { Left = result, Right = result };
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}
	}
}
