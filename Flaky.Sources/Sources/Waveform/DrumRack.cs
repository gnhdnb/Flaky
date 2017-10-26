using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class DR : Source, IPipingSource<NoteSource>
	{
		private string[] samples;
		private NoteSource noteSource;
		private IWaveReader[] readers;

		internal DR(params string[] samples)
		{
			this.samples = samples;
		}

		protected override Sample NextSample(IContext context)
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

			return result ?? 0;
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

		void IPipingSource<NoteSource>.SetMainSource(NoteSource mainSource)
		{
			this.noteSource = mainSource;
		}
	}
}
