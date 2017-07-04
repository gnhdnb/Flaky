using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class RSeq : Seq
	{
		private readonly static Random random = new Random();

		public RSeq(string sequence, Source length, string id) : base(sequence, length, id) { }
		public RSeq(string sequence, int size, string id) : base(sequence, size, id) { }
		public RSeq(IEnumerable<int> notes, Source length, string id) : base(notes, length, id) { }
		public RSeq(IEnumerable<int> notes, int size, string id) : base(notes, size, id) { }

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

	public class Seq : NoteSource
	{
		protected Note[] notes;
		private Source length;
		private int size;
		protected State state;

		protected class State
		{
			public int index = -1;
			public PlayingNote currentNote;
			public bool playing = false;
			public float latestLength;
			public int codeVersion;
		}

		public Seq(IEnumerable<int> notes, Source length) : this(notes.Select(n => (Note)n), length) { }

		public Seq(IEnumerable<int> notes, Source length, string id) : this(notes.Select(n => (Note)n), length, id) { }

		public Seq(IEnumerable<int> notes, int size, string id) : this(notes.Select(n => (Note)n), size, id) { }

		public Seq(IEnumerable<Note> notes, Source length)
		{
			this.notes = notes.ToArray();
			this.length = length;
		}

		public Seq(IEnumerable<Note> notes, Source length, string id) : base(id)
		{
			this.notes = notes.ToArray();
			this.length = length;
		}

		public Seq(IEnumerable<Note> notes, int size, string id) : base(id)
		{
			this.notes = notes.ToArray();
			length = null;

			this.size = size;
			if (size <= 0)
				size = 1;
		}

		public Seq(string sequence, Source length, string id) : base(id)
		{
			this.notes = ParseSequence(sequence);
			this.length = length;
		}

		public Seq(string sequence, int size, string id) : base(id)
		{
			this.notes = ParseSequence(sequence);
			length = null;

			this.size = size;
			if (size <= 0)
				size = 1;
		}

		public override PlayingNote GetNote(IContext context)
		{
			if (!state.playing && context.Beat % 4 == 0 && context.MetronomeTick)
				state.playing = true;

			if (!state.playing)
				return state.currentNote;

			if (state.currentNote.CurrentTime(context) > state.latestLength)
			{
				state.currentNote = NextNote(context, state);
				state.latestLength = GetLength(context);
				return state.currentNote;
			}

			return state.currentNote;
		}

		private float GetLength(IContext context)
		{
			if(length != null)
			{
				return length.Play(context).Value;
			}
			else
			{
				return 60 / ((float)context.BPM * size / 16);
			}
		}

		private PlayingNote NextNote(IContext context, State state)
		{
			state.index = GetNextNoteIndex(context, state);

			if (state.index >= notes.Length)
				state.index = 0;

			return new PlayingNote(notes[state.index], context.Sample);
		}

		protected virtual int GetNextNoteIndex(IContext context, State state)
		{
			return state.index + 1;
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);

			if(length != null)
				Initialize(context, length);
		}

        public override void Dispose()
        {
            Dispose(length);
        }

        private static Note[] ParseSequence(string sequence)
		{
			List<Note> result = new List<Note>();

			foreach(var key in sequence)
			{
				if (key == '-')
					result.Add(null);

				if (keys.ContainsKey(key.ToString()))
					result.Add(keys[key.ToString()]);
			}

			return result.ToArray();
		}

		private readonly static Dictionary<string, int> keys = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
		{
			{ "0", 0 },
			{ "1", 1 },
			{ "2", 2 },
			{ "3", 3 },
			{ "4", 4 },
			{ "5", 5 },
			{ "6", 6 },
			{ "7", 7 },
			{ "8", 8 },
			{ "9", 9 },
			{ "a", 10 },
			{ "b", 11 }
		};
	}
}
