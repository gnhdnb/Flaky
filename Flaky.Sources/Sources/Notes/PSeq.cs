using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class PSeq : PolyphonicNoteSource
	{
		private readonly Seq[] sequencers;
		private PlayingNote[] currentNotes;

		internal PSeq(string sequence, int size, string id)
		{
			sequencers = CreateSequencers(sequence, size, id);

			currentNotes = sequence.Select(s => new PlayingNote()).ToArray();
		}

		protected virtual Seq[] CreateSequencers(string sequence, int size, string id)
		{
			return sequence
				.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select((s, i) => new Seq(s, size, $"{id}-{i}"))
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

		public override void Initialize(IContext context)
		{
			Initialize(context, sequencers);
		}
	}

	public abstract class PolyphonicNoteSource : NoteSource
	{
		public abstract PlayingNote[] GetNotes(IContext context);

		protected abstract NoteSource[] Sources { get; }

		public sealed override PlayingNote GetNote(IContext context)
		{
			return GetNotes(context)[0];
		}

		public static Source operator %(PolyphonicNoteSource a, Func<NoteSource, int, Source> b)
		{
			return a.Sources.Select((s, i) => b(s, i)).Aggregate((r, s) => r + s);
		}
	}
}
