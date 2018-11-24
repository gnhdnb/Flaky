using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class FixLengthSequence : Sequence
	{
		public int size;

		internal FixLengthSequence(INoteCollection noteSource, int size, bool skipSilentNotes, string id)
			: base(noteSource, skipSilentNotes, id)
		{
			this.size = size;
		}

		protected override void MoveToNextNote(IContext context)
		{
			// do nothing
		}

		internal override bool NextNoteRequired(IContext context)
		{
			double sequenceLength = (context.Sample - state.startSample);
			double noteLength = (16 * 60 * context.SampleRate) / (double)(context.BPM * size);

			var expectedNextNoteIndex = Math.Ceiling(sequenceLength / noteLength) * noteLength;

			return expectedNextNoteIndex - sequenceLength <= 1;
		}
	}

	public class VariableLengthSequence : Sequence
	{
		private Source length;

		private LengthState lengthState;

		public class LengthState
		{
			public float lastSampledLength;
		}

		internal VariableLengthSequence(INoteCollection noteSource, Source length, bool skipSilentNotes, string id)
			: base(noteSource, skipSilentNotes, id)
		{
			this.length = length ?? throw new ArgumentNullException(nameof(length));
		}

		internal override bool NextNoteRequired(IContext context)
		{
			return state.currentSequencedNote.CurrentTime(context) > lengthState.lastSampledLength;
		}

		protected override void MoveToNextNote(IContext context)
		{
			lengthState.lastSampledLength = length.Play(context).Value;
		}

		public override void Initialize(IContext context)
		{
			lengthState = GetOrCreate<LengthState>(context);
		}

		public override void Dispose()
		{
			Dispose(length);

			base.Dispose();
		}
	}

	public abstract class Sequence : NoteSource
	{
		private INoteCollection noteSource;
		private bool skipSilentNotes;
		protected State state;

		public class State
		{
			public PlayingNote currentPlayingNote;
			public PlayingNote currentSequencedNote;
			public long startSample;
			public bool playing;
		}

		internal Sequence(INoteCollection noteSource, bool skipSilentNotes, string id) : base(id)
		{
			this.noteSource = noteSource ?? throw new ArgumentNullException(nameof(noteSource));
			this.skipSilentNotes = skipSilentNotes;
		}

		public override PlayingNote GetNote(IContext context)
		{
			noteSource.Update(context);

			if (!state.playing && context.Beat % 4 == 0 && context.MetronomeTick)
			{
				state.playing = true;
				state.startSample = context.Sample;
			}

			if (!state.playing)
				return state.currentPlayingNote;

			if (NextNoteRequired(context))
			{
				var nextNote = NextNote(context);

				state.currentSequencedNote = nextNote;

				if (!nextNote.IsSilent || !skipSilentNotes)
					state.currentPlayingNote = nextNote;
			}

			return state.currentPlayingNote;
		}

		internal bool Playing { get { return state.playing; } }

		internal abstract bool NextNoteRequired(IContext context);

		protected abstract void MoveToNextNote(IContext context);

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);

			noteSource.Initialize((IFlakyContext)context);
		}

		public override void Dispose()
		{
			noteSource.Dispose();
		}

		private PlayingNote NextNote(IContext context)
		{
			return new PlayingNote(noteSource.GetNextNote(), context.Sample);
		}
	}
}