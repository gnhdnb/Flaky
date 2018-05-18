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
		private Source hpNoiseSum;
		private Source hpNoiseDiff;
		private Osc lfo;
		private DetonationState state;
		private Analog analog;
		private Hold hold;
		private readonly float noiseLevel;

		private class DetonationState
		{
			internal Sample[] detonationBuffer = new Sample[44100];
			internal int detonationBufferPosition;
			internal long sample;

			public Sample NextSample(Sample stereo, float lfo)
			{
				return Detonation(stereo, lfo);
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

		internal Tape(string id) : this(1.0f, id) { }

		internal Tape(float noiseLevel, string id) : base(id)
		{
			if (noiseLevel < 0)
				noiseLevel = 0;

			if (noiseLevel > 1)
				noiseLevel = 1;

			this.noiseLevel = noiseLevel;

			lfo = new Osc(5, 1, $"{id}_lfo");
			hpNoiseSum = CreateNoiseChain($"{id}_hpNoiseSum");
			hpNoiseDiff = CreateNoiseChain($"{id}_hpNoiseDiff");
			hold = new Hold($"{id}_hold");
			analog = new Analog(hold, $"{id}_analog");
		}

		private Source CreateNoiseChain(string id)
		{
			Source result = new OnePoleHPFilter(new Noise(), 1f, $"{id}_hpNoise1");
			result = new OnePoleHPFilter(result, 1f, $"{id}_hpNoise2");

			return result;
		}

		public override void Dispose()
		{
			Dispose(hpNoiseSum, hpNoiseDiff, source, lfo, analog);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<DetonationState>(context);
			Initialize(context, source, hpNoiseSum, hpNoiseDiff, lfo, analog);
		}

		protected override Sample NextSample(IContext context)
		{
			var inputSample = source.Play(context);
			var sumNoiseSample = hpNoiseSum.Play(context);
			var diffNoiseSample = hpNoiseDiff.Play(context);
			var lfoSample = lfo.Play(context);

			var sum = inputSample.Left + inputSample.Right + sumNoiseSample.Value * 0.00004f * noiseLevel;
			var difference = inputSample.Left - inputSample.Right + diffNoiseSample.Value * 0.00001f * noiseLevel;

			hold.Sample = new Sample
			{
				Left = sum,
				Right = difference
			};

			var result = analog.Play(context);

			return state.NextSample(
				new Sample
				{
					Left = (result.Left + result.Right) / 2,
					Right = (result.Left - result.Right) / 2,
				},
				lfoSample.Value);
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}
	}
}
