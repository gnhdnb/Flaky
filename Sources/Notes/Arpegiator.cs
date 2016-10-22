using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public abstract class NoteSource : Source
	{
		public abstract PlayingNote GetNote(IContext context);

		public sealed override Sample Play(IContext context)
		{
			return GetNote(context);
		}
	}

	public class Arpegiator : NoteSource
	{
		private Note[] notes;
		private int index;
		private float length;
		private PlayingNote currentNote;

		public Arpegiator(IEnumerable<Note> notes, float length)
		{
			this.notes = notes.ToArray();
			this.length = length;
		}

		public override PlayingNote GetNote(IContext context)
		{
			if (currentNote.Note == null || currentNote.PlayTime(context) > length)
			{
				currentNote = NextNote(context);
				return currentNote;
			}

			return currentNote;
		}

		private PlayingNote NextNote(IContext context)
		{
			index++;

			if (index >= notes.Length)
				index = 0;

			return new PlayingNote(notes[index], context.Sample);
		}
	}
}
