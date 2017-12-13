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

		protected override Seq[] CreateSequencers(string sequence, int size, string id)
		{
			return sequence
				.Select(s => s.ToString())
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select((s, i) => new MonoGroover(s, $"{id}-{i}"))
				.ToArray();
		}

		private class MonoGroover : Seq
		{
			private Random random;

			public MonoGroover(string note, string id) : base(note + "--", 64, id)
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
