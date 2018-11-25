using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Groover : PSeq
	{
		internal Groover(string sequence, string id) 
			: base(sequence, 64, id)
		{
		}

		internal Groover(string sequence, int size, string id)
			: base(sequence, size, id)
		{
		}

		protected override Sequence[] CreateSequencers(string sequence, int size, string id)
		{
			return sequence
				.Select(s => s.ToString())
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select((s, i) => new FixLengthSequence(new GrooverNoteCollection(s, $"{id}-{i}"), size, false, $"{id}-{i}"))
				.ToArray();
		}

		private class GrooverNoteCollection : NoteCollection
		{
			private Random random;

			public GrooverNoteCollection(string note, string parentId) : base(note + "--", parentId)
			{
				random = new Random(parentId.GetHashCode());
			}

			protected override int GetNextNoteIndex(int currentNoteIndex)
			{
				if (currentNoteIndex == 0 && random.NextDouble() > 0.9)
				{
					return 0;
				}

				if (currentNoteIndex == 1 && random.NextDouble() > 0.7)
				{
					return 0;
				}

				if (currentNoteIndex == 2)
					return 0;

				return currentNoteIndex + 1;
			}
		}
	}
}
