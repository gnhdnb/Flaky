using System;
using System.Collections.Generic;

namespace Flaky
{
	public class RandomSeqence : Sequence
	{
		private readonly static Random random = new Random();

		public RandomSeqence(string sequence, Source length, string id) : base(sequence, length, id) { }
		public RandomSeqence(string sequence, int size, string id) : base(sequence, size, id) { }
		public RandomSeqence(IEnumerable<int> notes, Source length, string id) : base(notes, length, id) { }
		public RandomSeqence(IEnumerable<int> notes, int size, string id) : base(notes, size, id) { }

		public RandomSeqence(string sequence, Source length, bool skipSilentNotes, string id) : base(sequence, length, id)
		{
			SkipSilentNotes = skipSilentNotes;
		}

		protected override int GetNextNoteIndex(IContext context, State state)
		{
			int newIndex;

			if (notes.Length <= 1)
				return 0;

			do
			{
				newIndex = random.Next(0, notes.Length - 1);
			} while (newIndex == state.index);

			return newIndex;
		}
	}
}
