using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class SampleAndHold : Source, IPipingSource
	{
		private readonly Source trigger;
		private Source source;
		private State state;

		private class State
		{
			public Sample currentSample;
			public bool triggered = false;
		}

		internal SampleAndHold(Source trigger, string id) : base(id)
		{
			this.trigger = trigger;
		}

		protected override Sample NextSample(IContext context)
		{
			var sample = source.Play(context);
			var triggerSample = trigger.Play(context);

			if(triggerSample.Value < 0.5)
			{
				state.triggered = false;
			}

			if(triggerSample.Value >= 0.5 && !state.triggered)
			{
				state.currentSample = sample;
				state.triggered = true;
			}

			return state.currentSample;
		}

		public override void Dispose()
		{
			Dispose(trigger, source);
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			Initialize(context, trigger, source);
		}

		public void SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}
	}
}
