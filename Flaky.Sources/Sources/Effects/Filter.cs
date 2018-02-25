using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class LPFilter : MultiPoleFilter
	{
		internal LPFilter(Source cutoff, Source resonance, string id) : base(cutoff, resonance, id)
		{
		}

		protected override Source CreateFilterChain(Source input, Source cutoff, string id)
		{
			Source filterChain = new OnePoleLPFilter(input, cutoff, $"{id}_lp1");
			filterChain = new OnePoleLPFilter(filterChain, cutoff, $"{id}_lp2");
			filterChain = new OnePoleLPFilter(filterChain, cutoff, $"{id}_lp3");
			filterChain = new OnePoleLPFilter(filterChain, cutoff, $"{id}_lp4");

			return filterChain;
		}
	}

	public class BPFilter : MultiPoleFilter
	{
		internal BPFilter(Source cutoff, Source resonance, string id) : base(cutoff, resonance, id)
		{
		}

		protected override Source CreateFilterChain(Source input, Source cutoff, string id)
		{
			Source filterChain = new OnePoleLPFilter(input, cutoff, $"{id}_lp1");
			filterChain = new OnePoleHPFilter(filterChain, cutoff, $"{id}_hp2");
			filterChain = new OnePoleLPFilter(filterChain, cutoff, $"{id}_lp3");
			filterChain = new OnePoleHPFilter(filterChain, cutoff, $"{id}_hp4");

			return filterChain;
		}
	}

	public class HPFilter : MultiPoleFilter
	{
		internal HPFilter(Source cutoff, Source resonance, string id) : base(cutoff, resonance, id)
		{
		}

		protected override Source CreateFilterChain(Source input, Source cutoff, string id)
		{
			Source filterChain = new OnePoleHPFilter(input, cutoff, $"{id}_hp1");
			filterChain = new OnePoleHPFilter(filterChain, cutoff, $"{id}_hp2");
			filterChain = new OnePoleHPFilter(filterChain, cutoff, $"{id}_hp3");
			filterChain = new OnePoleHPFilter(filterChain, cutoff, $"{id}_hp4");

			return filterChain;
		}
	}


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
			this.feedbackChain = new Analog(feedbackChain, $"{id}_FeedbackAnalog");
		}

		protected abstract Source CreateFilterChain(Source input, Source cutoff, string id);

		public override void Initialize(IContext context)
		{
			Initialize(context, cutoff, source, filterChain, feedbackChain, resonance);
		}

		public override void Dispose()
		{
			Dispose(cutoff, source, filterChain, feedbackChain, resonance);
		}

		protected override Sample NextSample(IContext context)
		{
			var cutoffValue = cutoff.Play(context).Value;
			filterChainCutoffInput.Sample = cutoffValue;

			input.Sample = source.Play(context) + feedbackChain.Play(context);

			var output = filterChain.Play(context);
			var resonanceValue = resonance.Play(context).Value;

			if (Math.Abs(output.Left) > 100)
				output.Left = 100 * Math.Sign(output.Left);

			if (Math.Abs(output.Right) > 100)
				output.Right = 100 * Math.Sign(output.Right);

			feedbackInput.Sample = output * -GetResonance(resonanceValue, cutoffValue);

			return output;
		}

		private float GetResonance(float resonanceInput, float cutoff)
		{
			if (resonanceInput > 1)
				resonanceInput = 1;

			return 0.9f * resonanceInput * (1 - cutoff * cutoff + 0.1f);
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}
	}

	public class OnePoleLPFilter : OnePoleFilter
	{
		internal OnePoleLPFilter(Source source, Source cutoff, string id) : base(source, cutoff, id) { }

		internal OnePoleLPFilter(Source cutoff, string id) : base(cutoff, id) { }

		protected override Sample GetResult(Sample lp, Sample hp)
		{
			return lp;
		}
	}

	public class OnePoleHPFilter : OnePoleFilter
	{
		internal OnePoleHPFilter(Source source, Source cutoff, string id) : base(source, cutoff, id) { }
		internal OnePoleHPFilter(Source cutoff, string id) : base(cutoff, id) { }

		protected override Sample GetResult(Sample lp, Sample hp)
		{
			return hp;
		}
	}

	public abstract class OnePoleFilter : Source, IPipingSource
	{
		private Source source;
		private Source cutoff;
		private State state;

		private class State
		{
			public Sample integratorState;
			public Sample lp;
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

			var hp = sample - state.lp;
			state.lp = Integrate(hp * cutoffValue);
			return GetResult(state.lp, hp);
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
