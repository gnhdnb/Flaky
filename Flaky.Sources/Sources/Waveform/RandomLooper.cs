using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class RandomLooper : Source
	{
		private readonly string sample;
		private State state;
		private float delta;
		private int repetitions;

		private class State
		{
			public double LatestSamplerSample;
			public IWaveReader Reader;
		}

		public RandomLooper(string sample, int repetitions, float delta, string id) : base(id)
		{
			this.repetitions = repetitions;
			this.sample = sample;
			this.delta = delta;
		}

		protected override Vector2 NextSample(IContext context)
		{
			state.LatestSamplerSample += delta;

			if (state.LatestSamplerSample >= state.Reader.Length)
				state.LatestSamplerSample = 0;

			var result = state.Reader.Read((float)state.LatestSamplerSample);

			return result ?? new Vector2(0, 0);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			var factory = Get<IWaveReaderFactory>(context);

			if (state.Reader == null)
				state.Reader = new RandomLoopedReader(
					factory.Create(context, sample),
					repetitions,
					id);
		}

		public override void Dispose()
		{
			
		}

		private class RandomLoopedReader : IWaveReader
		{
			private Vector2[] sample;

			public RandomLoopedReader(IWaveReader reader, int repetitions, string id)
			{
				var random = new Random(id.GetHashCode());

				if (repetitions < 1)
					repetitions = 1;

				var length = reader.Length - reader.Length % 2;

				this.sample = new Vector2[length];

				float max = 0;

				for (int rep = 0; rep < repetitions; rep++)
				{
					long offset = (long)(random.NextDouble() * length) % length;

					for (long i = 0; i < length; i++)
					{
						var writeIndex = (i + offset) % length;
						sample[writeIndex] += reader.Read(i).Value;

						if (sample[writeIndex].Length() > max)
							max = sample[writeIndex].Length();
					}
				}

				if (max > 1)
				{
					for (long i = 0; i < length; i++)
					{
						sample[i] = sample[i] / max;
					}
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
	}
}
