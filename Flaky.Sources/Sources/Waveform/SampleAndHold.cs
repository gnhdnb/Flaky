using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class SampleAndHold : Source, IPipingSource
	{
		private readonly Source trigger;
		private readonly NoteSource noteTrigger;
		private Source source;
		private State state;

		private class State
		{
			public Vector2 currentSample;
			public bool triggered = false;
		}

		internal SampleAndHold(NoteSource trigger, string id) : base(id)
		{
			noteTrigger = trigger;
		}

		internal SampleAndHold(Source trigger, string id) : base(id)
		{
			this.trigger = trigger;
		}

		private bool Triggered(IContext context)
		{
			if (trigger != null)
			{
				var triggerSample = trigger.Play(context);

				return triggerSample.X >= 0.5;
			}
			else
			{
				var triggerNote = noteTrigger.GetNote(context);

				return triggerNote.CurrentSample(context) == 0;
			}
		}

		protected override Vector2 NextSample(IContext context)
		{
			var sample = source.Play(context);

			var triggered = Triggered(context);
			

			if(!triggered)
			{
				state.triggered = false;
			}

			if(triggered && !state.triggered)
			{
				state.currentSample = sample;
				state.triggered = true;
			}

			return state.currentSample;
		}

		public override void Dispose()
		{
			Dispose(trigger, noteTrigger, source);
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			Initialize(context, trigger, noteTrigger, source);
		}

		public void SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}
	}
}
