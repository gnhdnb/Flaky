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
		private bool isHighPass;

		private class State
		{
			public Vector2 integratorState;
			public Vector2 lp;
			public Vector2 latestInputSample;
		}

		internal OnePoleFilter(Source source, Source cutoff, bool isHighPass, string id) : base(id)
		{
			this.source = source;
			this.cutoff = cutoff;
			this.isHighPass = isHighPass;
		}

		internal OnePoleFilter(Source cutoff, bool isHighPass, string id) : base(id)
		{
			this.cutoff = cutoff;
			this.isHighPass = isHighPass;
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

		protected override Vector2 NextSample(IContext context)
		{
			var sample = source.Play(context);
			var cutoffValue = cutoff.Play(context).X * 0.25f;

			if (cutoffValue < 0)
				cutoffValue = 0;

			if (cutoffValue > 0.25f)
				cutoffValue = 0.25f;

			var hp = new Vector2(0, 0);
			var integrator = state.integratorState;
			var lp = state.integratorState;
			var latestInput = state.latestInputSample;

			const float oversamplingD = 1 / (float)oversampling;

			for (int i = 1; i <= oversampling; i++)
			{
				// lp = (s * i + latestInput * (oversampling - i)) - lp;
				if (i == 1)
					hp = sample + latestInput + latestInput + latestInput - lp;
				else if (i == 2)
					hp = sample + sample + latestInput + latestInput - lp;
				else if (i == 3)
					hp = sample + sample + sample + latestInput - lp;
				else if (i == 4)
					hp = sample + sample + sample + sample - lp;

				lp = hp * cutoffValue + integrator;
				integrator = hp * cutoffValue + lp;
			}

			state.latestInputSample = sample;
			state.lp = lp;
			state.integratorState = integrator;

			if(isHighPass)
				return hp * oversamplingD;
			else
				return lp * oversamplingD;
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}
	}
}
