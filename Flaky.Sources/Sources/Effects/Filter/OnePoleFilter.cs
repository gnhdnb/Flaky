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
			public Sample integratorState;
			public Sample lp;
			public Sample latestInputSample;
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

		protected override Sample NextSample(IContext context)
		{
			var sample = source.Play(context);
			var cutoffValue = cutoff.Play(context).Value;

			if (cutoffValue < 0)
				cutoffValue = 0;

			if (cutoffValue > 1)
				cutoffValue = 1;

			var hp = new Vector2(0, 0);
			var integrator = new Vector2(state.integratorState.Left, state.integratorState.Right);
			var lp = new Vector2(state.integratorState.Left, state.integratorState.Right);
			var s = new Vector2(sample.Left, sample.Right);
			var latestInput = new Vector2(state.latestInputSample.Left, state.latestInputSample.Right);

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
			state.lp = new Sample { Left = lp.X, Right = lp.Y };
			state.integratorState = new Sample { Left = integrator.X, Right = integrator.Y };

			return GetResult(state.lp, new Sample { Left = hp.X, Right = hp.Y });
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}

		protected abstract Sample GetResult(Sample lp, Sample hp);
	}
}
