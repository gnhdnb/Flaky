using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Delay : Source, IPipingSource
	{
		private readonly Source time;
		private Source sound;
		private readonly Hold feedback;
		private readonly Source transform;
		private readonly Source dryWet;
		private State state;

		private class State
		{
			internal int sampleRate;
			internal int capacity;
			internal Vector2[] buffer;
			internal int position;
			internal long sample;

			internal void Initialize(IContext context)
			{
				if (buffer != null)
					return;

				sampleRate = context.SampleRate;
				capacity = sampleRate * 10;
				buffer = new Vector2[capacity];
			}
		}

		internal Delay(Source time, string id) : this(time, null, id) { }
		internal Delay(Source time, Func<Source, Source> transform, string id) : this(time, transform, 0.5, id) { }

		internal Delay(Source time, Func<Source, Source> transform, Source dryWet, string id) : base(id)
		{
			this.feedback = new Hold($"{id}_feedbackHold");
			this.time = time;
			this.dryWet = dryWet;

			if (transform != null)
				this.transform = transform(feedback);
			else
				this.transform = feedback * 0.5;
		}

		protected override Vector2 NextSample(IContext context)
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

			var dryWetValue = dryWet.Play(context).X;

			if (dryWetValue < 0)
				dryWetValue = 0;

			if (dryWetValue > 1)
				dryWetValue = 1;

			return feedbackValue * dryWetValue + soundValue * (1 - dryWetValue);
		}

		private int GetWritePosition(IContext context, State state)
		{
			var timeValue = (int)(time.Play(context).X * state.sampleRate);

			if (timeValue <= 0)
				return state.position;
			if (timeValue >= state.capacity)
				timeValue = state.capacity - 2;

			var writePosition = state.position + timeValue;

			while (writePosition >= state.capacity)
				writePosition -= state.capacity;

			return writePosition;
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);

			state.Initialize(context);

			Initialize(context, time, sound, transform, dryWet);
		}

		public override void Dispose()
		{
			Dispose(time, sound, transform, dryWet);
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.sound = mainSource;
		}
	}
}
