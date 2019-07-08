using System;
using System.Collections.Generic;
using System.Text;

namespace Flaky
{
	public class LateReverb : FrequencyDomainOperator
	{
		private readonly Source length;
		private State state;

		private class State
		{
			public float[] leftPeaks = new float[FrequencyDomainOperator.State.framesCount];
			public float[] rightPeaks = new float[FrequencyDomainOperator.State.framesCount];

			public float[] leftPhase = new float[FrequencyDomainOperator.State.framesCount];
			public float[] rightPhase = new float[FrequencyDomainOperator.State.framesCount];
		}

		public LateReverb(Source length, int oversampling, string id) : base(oversampling, id)
		{
			this.length = length;
		}

		protected override float GetEffect(IContext context)
		{
			return length.Play(context).X;
		}

		protected override void Processor(float[] left, float[] right, float effect)
		{
			if (effect > 1)
				effect = 1;

			if (effect < 0)
				effect = 0;

			var rand = new Random();

			for (int i = 0; i < left.Length; i++)
			{
				state.leftPeaks[i] *= (0.9f + (0.1f * effect));
				state.rightPeaks[i] *= (0.9f + (0.1f * effect));

				if (Math.Abs(left[i]) > state.leftPeaks[i])
					state.leftPeaks[i] = Math.Abs(left[i]);

				if (Math.Abs(right[i]) > state.rightPeaks[i])
					state.rightPeaks[i] = Math.Abs(right[i]);

				state.leftPhase[i] += state.leftPeaks[i];
				state.rightPhase[i] += state.rightPeaks[i];

				state.leftPhase[i] = state.leftPhase[i] % 1;
				state.rightPhase[i] = state.rightPhase[i] % 1;

				left[i] = (float)(state.leftPeaks[i] * Math.Cos(2 * state.leftPhase[i] * Math.PI) * rand.NextDouble());
				right[i] = (float)(state.rightPeaks[i] * Math.Cos(2 * state.rightPhase[i] * Math.PI) * rand.NextDouble());
			}
		}

		public override void Dispose()
		{
			base.Dispose();

			Dispose(length);
		}

		protected override void Initialize(IContext context)
		{
			base.Initialize(context);

			state = GetOrCreate<State>(context);

			Initialize(context, length);
		}
	}
}
