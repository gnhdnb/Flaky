using System;
using System.Collections.Generic;
using System.Text;

namespace Flaky
{
	public class ValueAsNote : NoteSource, IPipingSource<ISource>
	{
		private State state;
		private ISource source;
		private int multipliter;

		internal ValueAsNote(int multipliter, string id) : base(id)
		{
			this.multipliter = multipliter;
		}

		internal class State
		{
			public PlayingNote currentNote;
		}

		public override PlayingNote GetNote(IContext context)
		{
			int noteNumber = (int)Math.Round(source.Play(context).X) * multipliter;

			if (state.currentNote.IsSilent || state.currentNote.Note.Number != noteNumber)
				state.currentNote = new PlayingNote(new Note(noteNumber), context.Sample);


			return state.currentNote;
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			Initialize(context, source);
		}

		public override void Dispose()
		{
			Dispose(source);
		}

		public void SetMainSource(ISource mainSource)
		{
			source = mainSource;
		}
	}
}
