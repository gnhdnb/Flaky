using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class SequenceDelay : NoteSource, IPipingSource<NoteSource>
	{
		private INoteSource source;
		private State state;
		private readonly int delay;

		private class State
		{
			public readonly Queue<PlayingNote> RecordedNotes = new Queue<PlayingNote>();
			public PlayingNote CurrentNote;
			
			public void Initialize(int delay)
			{
				var delta = delay - RecordedNotes.Count;

				if(delta > 0)
					Enumerable
						.Range(0, delta)
						.ToList()
						.ForEach(i => RecordedNotes.Enqueue(new PlayingNote()));

				if(delta < 0)
					Enumerable
						.Range(0, -delta)
						.ToList()
						.ForEach(i => RecordedNotes.Dequeue());
			}
		}

		internal SequenceDelay(int delay, string id) : base(id) { this.delay = delay; }

		public override void Dispose()
		{
			Dispose(source);
		}

		public override PlayingNote GetNote(IContext context)
		{
			var originalNote = source.GetNote(context);

			if(originalNote.CurrentSample(context) == 0)
			{
				state.RecordedNotes.Enqueue(originalNote);
				var noteToPlay = state.RecordedNotes.Dequeue();
				state.CurrentNote = new PlayingNote(noteToPlay.Note, context.Sample);
			}

			return state.CurrentNote;
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			state.Initialize(delay);
			Initialize(context, source);
		}

		public void SetMainSource(NoteSource mainSource)
		{
			this.source = mainSource;
		}
	}
}
