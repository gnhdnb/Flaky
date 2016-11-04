using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public abstract class NoteSource : Source
	{
		protected NoteSource() : base() { }

		protected NoteSource(string id) : base(id) { }

		public abstract PlayingNote GetNote(IContext context);

		public sealed override Sample Play(IContext context)
		{
			return GetNote(context);
		}
	}

	public class Arp : NoteSource
	{
		private Note[] notes;
		private Source length;
		private State state;

		private class State
		{
			public int index;
			public PlayingNote currentNote;
		}

		public Arp(IEnumerable<Note> notes, Source length)
		{
			this.notes = notes.ToArray();
			this.length = length;
		}

		public Arp(IEnumerable<Note> notes, Source length, string id) : base(id)
		{
			this.notes = notes.ToArray();
			this.length = length;
		}

		public override PlayingNote GetNote(IContext context)
		{
			var lengthValue = length.Play(context).Value;

			if (state.currentNote.Note == null || state.currentNote.CurrentTime(context) > lengthValue)
			{
				state.currentNote = NextNote(context, state);
				return state.currentNote;
			}

			return state.currentNote;
		}

		private PlayingNote NextNote(IContext context, State state)
		{
			state.index++;

			if (state.index >= notes.Length)
				state.index = 0;

			return new PlayingNote(notes[state.index], context.Sample);
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			Initialize(context, length);
		}
	}
}
