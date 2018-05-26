using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class PSeq : PolyphonicNoteSource
	{
		private readonly Sequence[] sequencers;
		private PlayingNote[] currentNotes;

		internal PSeq(string sequence, int size, string id)
		{
			sequencers = CreateSequencers(sequence, size, id);

			currentNotes = sequence.Select(s => new PlayingNote()).ToArray();
		}

		protected virtual Sequence[] CreateSequencers(string sequence, int size, string id)
		{
			return sequence
				.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select((s, i) => new Sequence(s, size, $"{id}-{i}"))
				.ToArray();
		}

		protected override NoteSource[] Sources => sequencers;

		public override void Dispose()
		{
			Dispose(sequencers);
		}

		public override PlayingNote[] GetNotes(IContext context)
		{
			if(sequencers[0].Playing && sequencers[0].NextNoteRequired(context))
			{
				currentNotes = sequencers.Select(s => s.GetNote(context)).ToArray();
			}

			return currentNotes;
		}

		protected override void Initialize(IContext context)
		{
			Initialize(context, sequencers);
		}
	}

	public abstract class PolyphonicNoteSource : NoteSource
	{
		public PolyphonicNoteSource() : base() { }

		public PolyphonicNoteSource(string id) : base(id) { }

		public abstract PlayingNote[] GetNotes(IContext context);

		protected abstract NoteSource[] Sources { get; }

		protected sealed override PlayingNote NextNote(IContext context)
		{
			return GetNotes(context)[0];
		}

		public static Source operator %(PolyphonicNoteSource a, Func<NoteSource, int, Source> b)
		{
			return a.Sources.Select((s, i) => b(s, i)).Aggregate((r, s) => r + s);
		}
	}
}
