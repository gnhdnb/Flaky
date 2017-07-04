using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class DR : Source
	{
		private string[] samples;
		private readonly NoteSource noteSource;
		private IWaveReader[] readers;

		public DR(NoteSource noteSource, params string[] samples)
		{
			this.samples = samples;
			this.noteSource = noteSource;
		}

		public override Sample Play(IContext context)
		{
			var note = noteSource.GetNote(context);
			var sample = note.CurrentSample(context);

			if (note.IsSilent)
				return new Sample();

			var index = note.Note.Number;

			if (index < 0)
				return new Sample();

			if (index >= readers.Length)
				return new Sample();

			var result = readers[index].Read(sample);

			return new Sample { Value = result ?? 0 };
		}

		public override void Initialize(IContext context)
		{
			var factory = Get<IWaveReaderFactory>(context);

			readers = samples.Select(s => factory.Create(s)).ToArray();

			Initialize(context, noteSource);
		}

        public override void Dispose()
        {
            Dispose(noteSource);
        }
    }
}
