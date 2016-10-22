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

		private class State
		{
			public int Index { get; set; }
			public PlayingNote CurrentNote { get; set; }
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
			var state = GetOrCreate<State>(context);

			var lengthValue = length.Play(context).Value;

			if (state.CurrentNote.Note == null || state.CurrentNote.PlayTime(context) > lengthValue)
			{
				state.CurrentNote = NextNote(context, state);
				return state.CurrentNote;
			}

			return state.CurrentNote;
		}

		private PlayingNote NextNote(IContext context, State state)
		{
			state.Index++;

			if (state.Index >= notes.Length)
				state.Index = 0;

			return new PlayingNote(notes[state.Index], context.Sample);
		}
	}
}
