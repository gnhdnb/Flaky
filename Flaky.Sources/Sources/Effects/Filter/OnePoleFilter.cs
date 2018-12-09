using System;
using System.Numerics;

namespace Flaky
{
	public abstract class OnePoleFilter : Source, IPipingSource
	{
		private Source source;
		private Source cutoff;
		private State state;
		private const int oversampling = 4;

		private class State
		{
			public Vector2 integratorState;
			public Vector2 lp;
			public Vector2 latestInputSample;
		}

		internal OnePoleFilter(Source source, Source cutoff, string id) : base(id)
		{
			this.source = source;
			this.cutoff = cutoff;
		}

		internal OnePoleFilter(Source cutoff, string id) : base(id)
		{
			this.cutoff = cutoff;
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			Initialize(context, source, cutoff);
		}

		public override void Dispose()
		{
			Dispose(source, cutoff);
		}

		protected override Vector2 NextSample(IContext context)
		{
			var sample = source.Play(context);
			var cutoffValue = cutoff.Play(context).X;

			if (cutoffValue < 0)
				cutoffValue = 0;

			if (cutoffValue > 1)
				cutoffValue = 1;

			var hp = new Vector2(0, 0);
			var integrator = state.integratorState;
			var lp = state.integratorState;
			var s = sample;
			var latestInput = state.latestInputSample;

			const float oversamplingD = 1 / (float)oversampling;

			for (int i = 1; i <= oversampling; i++)
			{
				hp = (s * i + latestInput * (oversampling - i)) * oversamplingD - lp;

				var input = hp * (cutoffValue * 0.25f);
				var output = input + integrator;

				integrator = input + output;

				lp = output;
			}

			state.latestInputSample = sample;
			state.lp = lp;
			state.integratorState = integrator;

			return GetResult(state.lp, hp);
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}

		protected abstract Vector2 GetResult(Vector2 lp, Vector2 hp);
	}
}
