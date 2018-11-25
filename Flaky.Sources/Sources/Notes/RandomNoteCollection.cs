using System;
using System.Collections.Generic;

namespace Flaky
{
	internal class RandomNoteCollection : NoteCollection
	{
		private readonly static Random random = new Random();

		public RandomNoteCollection(string sequence, string parentId) : base(sequence, parentId) { }

		protected override int GetNextNoteIndex(int currentNoteIndex)
		{
			int newIndex;

			if (sequence.Length <= 1)
				return 0;

			do
			{
				newIndex = random.Next(0, sequence.Length - 1);
			} while (newIndex == currentNoteIndex);

			return newIndex;
		}
	}
}
