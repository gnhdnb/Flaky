using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flaky
{
	public class Harmonize : NoteSource, IPipingSource<NoteSource>
	{
		private Note[] scale;
		private int offset;
		private int range;
		private bool empty;

		private INoteSource source;
		private PlayingNote lastSourceNote;
		private PlayingNote currentNote;

		public Harmonize(string scale, string id) : base(id)
		{
			this.scale = scale
				.AsNoteSequence()
				.OrderBy(s => s.Number)
				.Distinct()
				.ToArray();

			if (!scale.Any())
			{
				empty = true;
				return;
			}

			var min = this.scale.Min(n => n.Number);
			var max = this.scale.Max(n => n.Number);

			if (min >= 0)
			{
				offset = min - min % 12;
			}
			else
			{
				offset = min - min % 12 - 12;
				
			}

			if(max >= 0)
			{
				range = max - max % 12 + 12 - offset;
			}
			else
			{
				range = max - max % 12 - offset;
			}
		}

		public override PlayingNote GetNote(IContext context)
		{
			var sourceNote = source.GetNote(context);

			if (sourceNote != lastSourceNote)
			{
				currentNote = new PlayingNote(
					GetMatchingNoteFromScale(sourceNote.Note),
					sourceNote.StartSample);

				lastSourceNote = sourceNote;
			}

			return currentNote;
		}

		private Note GetMatchingNoteFromScale(Note note)
		{
			var noteNumberInRange = ((note.Number - offset) % range + range) % range + offset;

			var noteOffset = (note.Number - offset) / range;
			if (note.Number - offset < 0)
				noteOffset--;

			var nearestNote = scale
				.OrderBy(n => Math.Abs(n.Number - noteNumberInRange))
				.First();

			return new Note(nearestNote.Number + noteOffset * range);
		}

		protected override void Initialize(IContext context)
		{
			Initialize(context, source);
		}

		public override void Dispose()
		{
			Dispose(source);
		}

		public void SetMainSource(NoteSource mainSource)
		{
			source = mainSource;
		}
	}
}
