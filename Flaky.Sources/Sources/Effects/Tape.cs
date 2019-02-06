using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Tape : Source, IPipingSource
	{
		private Source source;
		private Source hpNoiseX;
		private Source hpNoiseY;
		private Osc lfo;
		private DetonationState state;
		private Analog analog;
		private Hold hold;
		private readonly float noiseLevel;

		private class DetonationState
		{
			internal Vector2[] detonationBuffer = new Vector2[44100];
			internal int detonationBufferPosition;
			internal long sample;

			public Vector2 NextSample(Vector2 stereo, float lfo)
			{
				return Detonation(stereo, lfo);
			}

			private Vector2 Detonation(Vector2 sample, float lfo)
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
			hpNoiseX = CreateNoiseChain($"{id}_hpNoiseX");
			hpNoiseY = CreateNoiseChain($"{id}_hpNoiseY");
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
			Dispose(hpNoiseX, hpNoiseY, source, lfo, hold, analog);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<DetonationState>(context);
			Initialize(context, hpNoiseX, hpNoiseY, source, lfo, analog, hold);
		}

		protected override Vector2 NextSample(IContext context)
		{
			var inputSample = source.Play(context);
			var xSample = hpNoiseX.Play(context);
			var ySample = hpNoiseY.Play(context);
			var lfoSample = lfo.Play(context);

			hold.Sample = new Vector2
			{
				X = inputSample.X + xSample.X * 0.00003f * noiseLevel,
				Y = inputSample.Y + ySample.X * 0.00003f * noiseLevel
			};

			var result = analog.Play(context);

			return state.NextSample(
				new Vector2
				{
					X = result.X,
					Y = result.Y,
				},
				lfoSample.X);
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}
	}
}
