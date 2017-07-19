﻿using System;
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

		public Rep(Source source, NoteSource feed, string id) : base(id)
		{
			this.source = source;
			this.feed = feed;
		}

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

			return state.buffer[readPosition] * 0.5f + sound;
		}

		private long GetReadPosition(IContext context, State state, PlayingNote note)
		{
			var sampleLength = (int)((2 - note.Note.ToPitch()) * context.SampleRate / 256 + 2);

			if (sampleLength < 16)
				sampleLength = 16;

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