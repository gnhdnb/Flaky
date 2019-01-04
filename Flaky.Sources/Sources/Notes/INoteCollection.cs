using System;
using System.Collections.Generic;

namespace Flaky
{
	internal interface INoteCollection
	{
		Note GetNextNote();
		void Reset();
		void Initialize(IFlakyContext context);

		void Update(IContext context);

		void Dispose();
	}

	internal class SequentialNoteCollection : NoteCollection
	{

		public SequentialNoteCollection(string sequence, string parentId)
			: base(sequence, parentId) { }

		protected override int GetNextNoteIndex(int currentNoteIndex)
		{
			var nextNote = currentNoteIndex + 1;

			if (nextNote >= sequence.Length)
				nextNote = 0;

			return nextNote;
		}
	}

	internal class OneShotSequentialNoteCollection : NoteCollection
	{
		public OneShotSequentialNoteCollection(string sequence, string parentId)
			: base(sequence, parentId) { }

		protected override int GetNextNoteIndex(int currentNoteIndex)
		{
			if (currentNoteIndex < -1)
				return currentNoteIndex;

			var nextNote = currentNoteIndex + 1;

			if (nextNote >= sequence.Length)
				nextNote = -2;

			return nextNote;
		}
	}

	internal abstract class NoteCollection : INoteCollection
	{
		protected Note[] sequence;
		protected State state;
		private string parentId;

		public class State
		{
			public int currentNoteIndex = -1;
		}

		public NoteCollection(string sequence, string parentId)
		{
			this.sequence = sequence.AsNoteSequence();
			this.parentId = parentId;
		}

		public Note GetNextNote()
		{
			state.currentNoteIndex = GetNextNoteIndex(state.currentNoteIndex);

			if (state.currentNoteIndex < 0)
				return null;

			return sequence[state.currentNoteIndex];
		}

		protected abstract int GetNextNoteIndex(int currentNoteIndex);

		public virtual void Initialize(IFlakyContext context)
		{
			state = context.GetOrCreateState<State>(parentId);
		}

		public virtual void Dispose()
		{
			// do nothing
		}

		public void Reset()
		{
			state.currentNoteIndex = -1;
		}

		public virtual void Update(IContext context)
		{
			// do nothing
		}
	}

	internal static class StringExtensions
	{
		public static Note[] AsNoteSequence(this string sequence)
		{
			List<Note> result = new List<Note>();

			for (int i = 0; i < sequence.Length; i++)
			{
				var key = sequence[i].ToString();

				if (key == "-")
					result.Add(null);

				if (keys.ContainsKey(key))
				{
					var note = keys[key];

					for (var j = i + 1; j < sequence.Length && modifiers.ContainsKey(sequence[j].ToString()); j++)
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