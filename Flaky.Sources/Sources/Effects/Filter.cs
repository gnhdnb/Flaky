using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class LPFilter : MultiPoleFilter
	{
		internal LPFilter(Source cutoff, Source resonance) : base(cutoff, resonance)
		{
		}

		protected override Source CreateFilterChain(Source input, Source cutoff)
		{
			Source filterChain = new OnePoleLPFilter(input, cutoff);
			filterChain = new OnePoleLPFilter(filterChain, cutoff);
			filterChain = new OnePoleLPFilter(filterChain, cutoff);
			filterChain = new OnePoleLPFilter(filterChain, cutoff);

			return filterChain;
		}
	}

	public class BPFilter : MultiPoleFilter
	{
		internal BPFilter(Source cutoff, Source resonance) : base(cutoff, resonance)
		{
		}

		protected override Source CreateFilterChain(Source input, Source cutoff)
		{
			Source filterChain = new OnePoleLPFilter(input, cutoff);
			filterChain = new OnePoleHPFilter(filterChain, cutoff);
			filterChain = new OnePoleLPFilter(filterChain, cutoff);
			filterChain = new OnePoleHPFilter(filterChain, cutoff);

			return filterChain;
		}
	}

	public class HPFilter : MultiPoleFilter
	{
		internal HPFilter(Source cutoff, Source resonance) : base(cutoff, resonance)
		{
		}

		protected override Source CreateFilterChain(Source input, Source cutoff)
		{
			Source filterChain = new OnePoleHPFilter(input, cutoff);
			filterChain = new OnePoleHPFilter(filterChain, cutoff);
			filterChain = new OnePoleHPFilter(filterChain, cutoff);
			filterChain = new OnePoleHPFilter(filterChain, cutoff);

			return filterChain;
		}
	}


	public abstract class MultiPoleFilter : Source, IPipingSource
	{
		private Hold input;
		private Hold filterChainCutoffInput;
		private Hold feedbackInput;
		private Source filterChain;
		private Source resonance;
		private Source source;
		private Source cutoff;
		private Source feedbackChain;

		internal MultiPoleFilter(Source cutoff, Source resonance)
		{
			this.resonance = resonance;
			this.input = new Hold();
			this.cutoff = cutoff;
			this.filterChainCutoffInput = new Hold();
			this.filterChain = CreateFilterChain(input, filterChainCutoffInput);
			this.feedbackInput = new Hold();
			this.feedbackChain = new OnePoleLPFilter(feedbackInput, 0.18f);
		}

		protected abstract Source CreateFilterChain(Source input, Source cutoff);

		public override void Initialize(IContext context)
		{
			Initialize(context, cutoff, source, filterChain, resonance);
		}

		public override void Dispose()
		{
			Dispose(cutoff, source, filterChain, resonance);
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

			return resonanceInput / ((float)Math.Sqrt(Math.Abs(cutoff)) + 0.15f - cutoff * 0.25f);
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}
	}

	public class OnePoleLPFilter : OnePoleFilter
	{
		internal OnePoleLPFilter(Source source, Source cutoff) : base(source, cutoff) { }

		internal OnePoleLPFilter(Source cutoff) : base(cutoff) { }

		protected override Sample GetResult(Sample lp, Sample hp)
		{
			return lp;
		}
	}

	public class OnePoleHPFilter : OnePoleFilter
	{
		internal OnePoleHPFilter(Source source, Source cutoff) : base(source, cutoff) { }
		internal OnePoleHPFilter(Source cutoff) : base(cutoff) { }

		protected override Sample GetResult(Sample lp, Sample hp)
		{
			return hp;
		}
	}

	public abstract class OnePoleFilter : Source, IPipingSource
	{
		private Source source;
		private Source cutoff;

		private Sample integratorState;
		private Sample lp;

		internal OnePoleFilter(Source source, Source cutoff)
		{
			this.source = source;
			this.cutoff = cutoff;
		}

		internal OnePoleFilter(Source cutoff)
		{
			this.cutoff = cutoff;
		}

		public override void Initialize(IContext context)
		{
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

			var hp = sample - lp;
			lp = Integrate(hp * cutoffValue);
			return GetResult(lp, hp);
		}

		private Sample Integrate(Sample sample)
		{
			var input = sample / 2;
			var output = input + integratorState;
			integratorState = input + output;
			return output;
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}

		protected abstract Sample GetResult(Sample lp, Sample hp);
	}
}
