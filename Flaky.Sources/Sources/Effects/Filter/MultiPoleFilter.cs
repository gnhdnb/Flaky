﻿using System;
using System.Numerics;

namespace Flaky
{
	public abstract class MultiPoleFilter : Source, IPipingSource
	{
		private Hold input;
		private Hold filterChainCutoffInput;
		private Hold feedbackInput;
		private Source feedbackChain;
		private Source filterChain;		

		private Source resonance;
		private Source source;
		private Source cutoff;

		internal MultiPoleFilter(Source cutoff, Source resonance, string id) : base(id)
		{
			this.resonance = resonance;
			this.input = new Hold($"{id}_InputHold");
			this.cutoff = cutoff;
			this.filterChainCutoffInput = new Hold($"{id}_FilterChainCutoffInputHold");
			this.filterChain = CreateFilterChain(input, filterChainCutoffInput, id);
			this.feedbackInput = new Hold($"{id}_FeedbackInputHold");
			this.feedbackChain = new OnePoleLPFilter(feedbackInput, 0.18f, $"{id}_OnePoleFeedback");
			this.feedbackChain = new Analog(feedbackChain, $"{id}_FeedbackAnalog") * 10;
		}

		protected abstract Source CreateFilterChain(Source input, Source cutoff, string id);

		protected override void Initialize(IContext context)
		{
			Initialize(context, cutoff, source, filterChain, feedbackChain, resonance);
		}

		public override void Dispose()
		{
			Dispose(cutoff, source, filterChain, feedbackChain, resonance);
		}

		protected override Vector2 NextSample(IContext context)
		{
			var cutoffValue = cutoff.Play(context);
			filterChainCutoffInput.Sample = cutoffValue;

			input.Sample = source.Play(context) + feedbackChain.Play(context);

			var output = filterChain.Play(context);
			var resonanceValue = resonance.Play(context).X;

			if (Math.Abs(output.X) > 100)
				output.X = 100 * Math.Sign(output.X);

			if (Math.Abs(output.Y) > 100)
				output.Y = 100 * Math.Sign(output.Y);

			feedbackInput.Sample = output * -GetResonance(resonanceValue, cutoffValue.X);

			return output * 4;
		}

		private float GetResonance(float resonanceInput, float cutoff)
		{
			if (resonanceInput > 1)
				resonanceInput = 1;

			return 0.9f * resonanceInput * (1 - cutoff * cutoff * 0.9f);
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}
	}
}
