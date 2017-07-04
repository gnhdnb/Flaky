using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Delay : Source
	{
		private readonly Source time;
		private readonly Source sound;
		private readonly Hold feedback;
		private readonly Source transform;
		private readonly Source dryWet;
		private State state;

		private class State
		{
			internal int sampleRate;
			internal int capacity;
			internal Sample[] buffer;
			internal int position;
			internal long sample;

			internal void Initialize(IContext context)
			{
				if (buffer != null)
					return;

				sampleRate = context.SampleRate;
				capacity = sampleRate * 10;
				buffer = new Sample[capacity];
			}
		}

		public Delay(Source sound, Source time) : this(sound, time, null) { }

		public Delay(Source sound, Source time, string id) : this(sound, time, null, id) { }
		public Delay(Source sound, Source time, Func<Source, Source> transform, string id) : this(sound, time, null, 0.5, id) { }

		public Delay(Source sound, Source time, Func<Source, Source> transform, Source dryWet, string id) : base(id)
		{
			this.feedback = new Hold();
			this.time = time;
			this.sound = sound;
			this.dryWet = dryWet;

			if (transform != null)
				this.transform = transform(feedback);
			else
				this.transform = feedback * 0.5;
		}

		public override Sample Play(IContext context)
		{
			var soundValue = sound.Play(context);

			var delta = context.Sample - state.sample;
			state.sample = context.Sample;

			state.position += (int)delta;

			while (state.position >= state.capacity)
				state.position -= state.capacity;

			var writePosition = GetWritePosition(context, state);

			var delay = state.buffer[state.position];

			feedback.Sample = delay;

			var feedbackValue = transform.Play(context);

			var result = feedbackValue + soundValue;

			state.buffer[writePosition] = result;

			var dryWetValue = dryWet.Play(context).Value;

			if (dryWetValue < 0)
				dryWetValue = 0;

			if (dryWetValue > 1)
				dryWetValue = 1;

			return feedbackValue * dryWetValue + soundValue * (1 - dryWetValue);
		}

		private int GetWritePosition(IContext context, State state)
		{
			var timeValue = (int)(time.Play(context).Value * state.sampleRate);

			if (timeValue <= 0)
				return state.position;
			if (timeValue >= state.capacity)
				timeValue = state.capacity - 2;

			var writePosition = state.position + timeValue;

			while (writePosition >= state.capacity)
				writePosition -= state.capacity;

			return writePosition;
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);

			state.Initialize(context);

			Initialize(context, time, sound, transform, dryWet);
		}

        public override void Dispose()
        {
            Dispose(time, sound, transform, dryWet);
        }
    }
}
