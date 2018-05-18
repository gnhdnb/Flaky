using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Sequence : NoteSource
	{
		protected Note[] notes;
		private Source length;
		private int size;
		protected State state;

		protected bool SkipSilentNotes { get; set; } = true;

		protected class State
		{
			public int index = -1;
			public PlayingNote currentNote;
			public bool playing = false;
			public float latestLength;
			public int codeVersion;
			public long currentSample;
			public long startSample;
		}

		public Sequence(IEnumerable<int> notes, Source length) : this(notes.Select(n => (Note)n), length) { }

		public Sequence(IEnumerable<int> notes, Source length, string id) : this(notes.Select(n => (Note)n), length, id) { }

		public Sequence(IEnumerable<int> notes, int size, string id) : this(notes.Select(n => (Note)n), size, id) { }

		public Sequence(IEnumerable<Note> notes, Source length)
		{
			this.notes = notes.ToArray();
			this.length = length;
		}

		public Sequence(IEnumerable<Note> notes, Source length, string id) : base(id)
		{
			this.notes = notes.ToArray();
			this.length = length;
		}

		public Sequence(IEnumerable<Note> notes, int size, string id) : base(id)
		{
			this.notes = notes.ToArray();
			length = null;

			this.size = size;
			if (size <= 0)
				size = 1;
		}

		public Sequence(string sequence, Source length, string id) : base(id)
		{
			this.notes = ParseSequence(sequence);
			this.length = length;
		}

		public Sequence(string sequence, Source length, bool skipSilentNotes, string id) : base(id)
		{
			this.notes = ParseSequence(sequence);
			this.length = length;
			SkipSilentNotes = skipSilentNotes;
		}

		public Sequence(string sequence, int size, string id) : base(id)
		{
			this.notes = ParseSequence(sequence);
			length = null;

			this.size = size;
			if (size <= 0)
				size = 1;
		}

		public Sequence(string sequence, int size, bool skipSilentNotes, string id) : this(sequence, size, id)
		{
			SkipSilentNotes = skipSilentNotes;
		}

		public override PlayingNote GetNote(IContext context)
		{
			var lengthValue = GetLength(context);

			Update(context);

			if (state.currentSample == context.Sample)
				return state.currentNote;

			state.currentSample = context.Sample;

			if (!state.playing && context.Beat % 4 == 0 && context.MetronomeTick)
			{
				state.playing = true;
				state.startSample = context.Sample;
			}

			if (!state.playing)
				return state.currentNote;

			if (NextNoteRequired(context))
			{
				var nextNote = NextNote(context, state);

				if(!nextNote.IsSilent || !SkipSilentNotes)
				{
					state.currentNote = nextNote;
					state.latestLength = lengthValue;
				} else
				{
					state.latestLength += lengthValue;
				}
				
				return state.currentNote;
			}

			return state.currentNote;
		}

		internal bool Playing { get { return state.playing; } }

		internal bool NextNoteRequired(IContext context)
		{
			if (length != null)
			{
				return state.currentNote.CurrentTime(context) > state.latestLength;
			}
			else
			{
				return ((context.Sample - state.startSample) % ((16 * 60 * context.SampleRate) / (context.BPM * size))) == 0;
			}
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

		protected virtual void Update(IContext context)
		{

		}

		protected override void Initialize(IContext context)
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

			for(int i = 0; i < sequence.Length; i++)
			{
				var key = sequence[i].ToString();

				if (key == "-")
					result.Add(null);

				if (keys.ContainsKey(key))
				{
					var note = keys[key];

					for(var j = i + 1; j < sequence.Length && modifiers.ContainsKey(sequence[j].ToString()); j++)
					{
						note += modifiers[sequence[j].ToString()];
					}

					result.Add(note);
				}
			}

			return result.ToArray();
		}

		private readonly static Dictionary<string, int> modifiers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
		{
			{ "u", 12 },
			{ "d", -12 },
		};

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
