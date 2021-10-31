using System;
using System.IO;
using System.Numerics;

namespace Flaky
{
	public class HarpsichordSource : Source
	{
		private readonly static double ratio = Math.Pow(2, 1d / 12d);
		private readonly Source clip;
		private readonly Source width;
		private readonly Source bass;
		private readonly Source crossFade;
		private readonly Source delta;
		private readonly Source deltaMul;
		private readonly ChromaticFieldSource field;
		private readonly string pack;

		private class State
		{
			public Harpsichord harp;
			public float[] bandAmp;
			private bool initialized;

			public void Initialize(IContext context, string pack)
			{
				if (initialized)
					return;

				harp = new Harpsichord(pack, context);
				bandAmp = new float[60];

				initialized = true;
			}
		}

		private State state;

		public HarpsichordSource(string pack,
			ChromaticFieldSource field,
			Source clip,
			Source width,
			Source bass,
			Source crossFade,
			Source delta,
			Source deltaMul,
			string id) : base(id)
		{
			this.pack = pack;
			this.clip = clip;
			this.bass = bass;
			this.delta = delta;
			this.deltaMul = deltaMul;
			this.crossFade = crossFade;
			this.width = width;
			this.field = field;
		}

		public override void Dispose()
		{
			Dispose(clip, width, bass, field, crossFade, delta, deltaMul);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			state.Initialize(context, pack);
			Initialize(context, clip, width, bass, field, crossFade, delta, deltaMul);
		}

		protected override Vector2 NextSample(IContext context)
		{
			var result = Vector2.Zero;

			var ampArray = field.GetField(context);

			var cf = crossFade.Play(context).X;

			var clipValue = clip.Play(context).X;

			var widthValue = width.Play(context).X;

			var bassValue = bass.Play(context).X;

			var deltaValue = delta.Play(context).X;

			var deltaMulValue = deltaMul.Play(context).X;

			for (int band = 0; band < 59; band++)
			{
				var amp = ampArray[band];

				if (amp < widthValue)
					amp = amp * amp * amp;

				if (amp > 1.5f)
					amp = 1.5f;

				if (amp > clipValue * 2)
					amp = 0;

				if (amp < 0)
					amp = 0;

				if (band > 40)
					amp = amp * bassValue * 2;

				state.bandAmp[band] = state.bandAmp[band] +
					(amp - state.bandAmp[band]) * ((1 - cf) + cf/(50 * 3500 + 100 * (band % 10)));
				//state.bandAmp[band] = amp;

				/*var c = 1 - (context.Sample % ((band + 50) * 100)) / ((band + 50) * 100.0f);

				if (c > 0.9)
					c = (1 - c) * 10f;*/

				var c = 1 / (float)Math.Pow(ratio, (60 - band) * (1 - deltaMulValue));

				if (band > 1)
					result += state.harp.Read(60 - band - 1, deltaValue * c) * state.bandAmp[band];
			}

			return result;
		}
	}

	public class Harpsichord
	{
		private readonly IWaveReader[] readers;
		private readonly float[] counters;

		private class LoopedReader : IWaveReader
		{
			private Vector2[] sample;

			public LoopedReader(IWaveReader reader)
			{
				var length = reader.Length - reader.Length % 2;

				this.sample = new Vector2[length];

				for (long i = 0; i < length; i++)
				{
					var cf = (float)(Math.Abs(2 * i - length) / (double)length);

					var a = reader.Read(i).Value;
					var b = reader.Read((i + length / 2) % length).Value;

					sample[i] = a * (1 - cf) + b * cf;
				}
			}

			public long Length => sample.LongLength;

			public Vector2[] Sample => sample;

			public Vector2? Read(long index)
			{
				if (index < sample.LongLength)
					return sample[index];
				else
					return null;
			}

			public Vector2? Read(float index)
			{
				var cf = index % 1;
				var i1 = (int)Math.Floor(index);
				var i2 = i1 + 1;

				i2 = i2 % sample.Length;
				i1 = i1 % sample.Length;

				return sample[i1] * (1 - cf) + sample[i2] * cf;
			}
		}

		public Harpsichord(string path, IContext context)
		{
			readers = new IWaveReader[59];
			counters = new float[59];

			for (int i = 0; i <= 58; i++)
			{
				readers[i] = new LoopedReader(((IFlakyContext)context).Get<IWaveReaderFactory>()
					.Create(context, Path.Combine(path, $"{i}")));
			}
		}

		public Vector2 Read(int band, float delta)
		{
			counters[band] += delta;

			if (counters[band] >= readers[band].Length)
				counters[band] = 0;

			return readers[band].Read(counters[band]).Value;
		}
	}
}
