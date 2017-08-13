using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Rep : Source
	{
		private readonly NoteSource feed;
		private readonly Source source;
		private State state;
		private readonly float multiplier;
		private readonly float dry;
		private readonly float wet;

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

		public Rep(Source source, NoteSource feed, float multiplier, float dry, float wet, string id) : base(id)
		{
			this.source = source;
			this.feed = feed;

			if (wet < 0)
				wet = 0;

			if (dry < 0)
				dry = 0;

			if (multiplier < 0)
				multiplier = 0;

			this.multiplier = multiplier;
			this.wet = wet;
			this.dry = dry;
		}

		public Rep(Source source, NoteSource feed, string id) : this(source, feed, 1, 1, 0.5f, id) { }

		public override Sample Play(IContext context)
		{
			var sound = source.Play(context);
			var note = feed.GetNote(context);

			var delta = context.Sample - state.sample;
			state.sample = context.Sample;
			state.position += (int)delta;

			while (state.position >= state.capacity)
				state.position -= state.capacity;

			state.buffer[state.position] = sound;

			if (note.IsSilent)
				return sound;

			var readPosition = GetReadPosition(context, state, note);

			return state.buffer[readPosition] * wet + sound * dry;
		}

		private long GetReadPosition(IContext context, State state, PlayingNote note)
		{
			var sampleLength = (int)(((2 - note.Note.ToPitch()) * context.SampleRate / 256 + 2) * multiplier);

			if (sampleLength < 4)
				sampleLength = 4;

			var readPosition = state.position - note.CurrentSample(context) + note.CurrentSample(context) % sampleLength;

			while (readPosition < 0)
				readPosition += state.capacity;

			while (readPosition >= state.capacity)
				readPosition -= state.capacity;

			return readPosition;
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			state.Initialize(context);

			Initialize(context, source, feed);
		}

        public override void Dispose()
        {
            Dispose(source, feed);
        }
    }
}
