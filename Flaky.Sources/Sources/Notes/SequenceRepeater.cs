using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class SequenceRepeater : NoteSource, IPipingSource<NoteSource>
	{
		private INoteSource mainSource;
		private readonly int blockSize;
		private readonly int repetitions;
		private State state;

		private class State
		{
			public List<Note> notes = new List<Note>();
			public int currentNote;
			public int currentPlay;
			public long lastNoteStartSample = -1;
		}

		public SequenceRepeater(int blockSize, int repetitions, string id) : base(id)
		{
			this.blockSize = blockSize;
			this.repetitions = repetitions;
		}

		public override void Dispose()
		{
			Dispose(mainSource);
		}

		public override PlayingNote GetNote(IContext context)
		{
			var sourcePlayingNote = mainSource.GetNote(context);

			if (sourcePlayingNote.StartSample != state.lastNoteStartSample)
			{
				if (state.currentPlay >= repetitions)
				{
					state.notes.Clear();
					state.currentPlay = 0;
				}

				if (state.notes.Count < blockSize)
				{
					state.notes.Add(sourcePlayingNote.Note);
				}

				state.currentNote++;

				if (state.currentNote >= state.notes.Count)
				{
					state.currentNote = 0;

					state.currentPlay++;
				}

				state.lastNoteStartSample = sourcePlayingNote.StartSample;
			}

			return state.notes.Count > 0
				? new PlayingNote(state.notes[state.currentNote], sourcePlayingNote.StartSample)
				: sourcePlayingNote;
		}

		protected override void Initialize(IContext context)
		{
			this.state = GetOrCreate<State>(context);
			Initialize(context, mainSource);
		}

		void IPipingSource<NoteSource>.SetMainSource(NoteSource mainSource)
		{
			this.mainSource = mainSource;
		}
	}
}
