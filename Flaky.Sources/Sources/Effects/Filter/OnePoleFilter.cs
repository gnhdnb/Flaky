using System;

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

		protected override void Initialize(IContext context)
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

			Sample hp = 0;

			for(int i = 1; i <= oversampling; i++)
			{
				var inputSample = 
					(sample * i + state.latestInputSample * (oversampling - i))
					* (1 / (float)oversampling);

				hp = Iterate(inputSample, cutoffValue / 2);
			}

			return GetResult(state.lp, hp);
		}

		private Sample Iterate(Sample inputSample, float cutoffValue)
		{
			var hp = inputSample - state.lp;
			state.lp = Integrate(hp * cutoffValue);
			return hp;
		}

		private Sample Integrate(Sample sample)
		{
			var input = sample / 2;
			var output = input + state.integratorState;
			state.integratorState = input + output;
			return output;
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}

		protected abstract Sample GetResult(Sample lp, Sample hp);
	}
}
