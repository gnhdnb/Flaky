using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class SequenceWrapper
	{
		private string sequence;

		internal SequenceWrapper(string sequence)
		{
			this.sequence = sequence;
		}

		public static NoteSource operator %(SequenceWrapper sequenceWrapper, int size)
		{
			var id = $"sequence{sequenceWrapper.sequence.Replace(" ", "")}size{size}";

			return new FixLengthSequence(
				new SequentialNoteCollection(sequenceWrapper.sequence, id), 
				size, 
				false,
				id);
		}
	}
}
