using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Sampler : Source
	{
		private readonly string sample;
		private readonly NoteSource noteSource;
		private IWaveReader reader;

		public Sampler(string sample, NoteSource noteSource)
		{
			this.sample = sample;
			this.noteSource = noteSource;
		}

		public override Sample Play(IContext context)
		{
			var note = noteSource.GetNote(context);

			var result = reader.Read(note.CurrentSample(context));

			return new Sample { Value = result ?? 0 };
		}

		internal override void Initialize(IContext context)
		{
			var factory = Get<IWaveReaderFactory>(context);

			reader = factory.Create(sample);

			Initialize(context, noteSource);
		}
	}
}
