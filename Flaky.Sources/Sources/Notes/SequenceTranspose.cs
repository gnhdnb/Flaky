using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class SequenceTranspose : NoteSource, IPipingSource<NoteSource>
	{
		private INoteSource mainSource;
		private int? delta;
		private INoteSource deltaSource;
		private Note currentNote;

		public SequenceTranspose(int delta)
		{
			this.delta = delta;
		}

		public SequenceTranspose(NoteSource delta)
		{
			this.deltaSource = delta;
		}

		public override void Dispose()
		{
			Dispose(mainSource, deltaSource);
		}

		public override PlayingNote GetNote(IContext context)
		{
			var mainNote = mainSource.GetNote(context);

			if (mainNote.Note == null)
				return mainNote;

			var d =
				delta != null
				? delta.Value
				: deltaSource.GetNote(context).Note?.Number ?? 0;

			if(currentNote == null
				|| currentNote.Number != mainNote.Note.Number + d)
			{
				currentNote = new Note(mainNote.Note.Number + d);
			}

			return new PlayingNote(currentNote, mainNote.StartSample);
		}

		protected override void Initialize(IContext context)
		{
			Initialize(context, mainSource, deltaSource);
		}

		void IPipingSource<NoteSource>.SetMainSource(NoteSource mainSource)
		{
			this.mainSource = mainSource;
		}
	}
}
