using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Looper : Source
	{
		private readonly string sample;
		private IWaveReader reader;
		private State state;

		private class State
		{
			public double LatestSamplerSample;
		}

		public Looper(string sample, string id) : base(id)
		{
			this.sample = sample;
		}

		public override Sample Play(IContext context)
		{
			var delta = 1;

			state.LatestSamplerSample = state.LatestSamplerSample + delta;

			if (state.LatestSamplerSample >= reader.Length)
				state.LatestSamplerSample = 0;

			var result = reader.Read((long)state.LatestSamplerSample);

			return result ?? 0;
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			var factory = Get<IWaveReaderFactory>(context);

			reader = factory.Create(sample);
		}

		public override void Dispose()
		{
			
		}
	}
}
