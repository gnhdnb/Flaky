﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Looper : Source
	{
		private readonly string sample;
		private State state;
		private float delta;
		private bool seamless;

		private class State
		{
			public double LatestSamplerSample;
			public IWaveReader Reader;
		}

		public Looper(string sample, float delta, bool seamless, string id) : base(id)
		{
			this.sample = sample;
			this.delta = delta;
			this.seamless = seamless;
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

			if(state.Reader == null)
				if(!seamless)
					state.Reader = factory.Create(context, sample);
				else
					state.Reader = new LoopedReader(factory.Create(context, sample));
		}

		public override void Dispose()
		{
			
		}
	}
}
