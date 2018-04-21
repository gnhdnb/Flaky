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
				.Select((s, i) => new MonoGroover(s, size, $"{id}-{i}"))
				.ToArray();
		}

		private class MonoGroover : Sequence
		{
			private Random random;

			public MonoGroover(string note, int size, string id) : base(note + "--", size, id)
			{
				random = new Random(id.GetHashCode());
			}

			protected override int GetNextNoteIndex(IContext context, State state)
			{
				if(state.index == 0 && random.NextDouble() > 0.9)
				{
					return 0;
				}

				if (state.index == 1 && random.NextDouble() > 0.7)
				{
					return 0;
				}

				if (state.index == 2)
					return 0;

				return state.index + 1;
			}
		}
	}
}
