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

			float hpLeft = 0;
			float hpRight = 0;

			const float oversamplingD = 1 / (float)oversampling;
			float integratorStateLeft = state.integratorState.Left;
			float integratorStateRight = state.integratorState.Right;
			float lpLeft = state.integratorState.Left;
			float lpRight = state.integratorState.Right;
			float sampleLeft = sample.Left;
			float sampleRight = sample.Right;
			float latestInputSampleLeft = state.latestInputSample.Left;
			float latestInputSampleRight = state.latestInputSample.Right;

			for (int i = 1; i <= oversampling; i++)
			{
				var inputSampleLeft = (sampleLeft * i + latestInputSampleLeft * (oversampling - i)) * oversamplingD;
				var inputSampleRight = (sampleRight * i + latestInputSampleRight * (oversampling - i)) * oversamplingD;

				hpLeft = inputSampleLeft - lpLeft;
				hpRight = inputSampleRight - lpRight;

				var inputLeft = hpLeft * (cutoffValue * 0.25f);
				var inputRight = hpRight * (cutoffValue * 0.25f);
				var outputLeft = inputLeft + integratorStateLeft;
				var outputRight = inputRight + integratorStateRight;

				integratorStateLeft = inputLeft + outputLeft;
				integratorStateRight = inputRight + outputRight;

				lpLeft = outputLeft;
				lpRight = outputRight;
			}

			state.latestInputSample = sample;
			state.lp = new Sample { Left = lpLeft, Right = lpRight };
			state.integratorState = new Sample { Left = integratorStateLeft, Right = integratorStateRight };

			return GetResult(state.lp, new Sample { Left = hpLeft, Right = hpRight });
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}

		protected abstract Sample GetResult(Sample lp, Sample hp);
	}
}
