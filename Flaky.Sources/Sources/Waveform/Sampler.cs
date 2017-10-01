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
		private readonly Source pitchSource;
		private IWaveReader reader;
		private State state;

		private class State
		{
			public long LatestNoteSample;
			public double LatestSamplerSample;
		}

		public Sampler(string sample, NoteSource noteSource, string id) : base(id)
		{
			this.sample = sample;
			this.noteSource = noteSource;
		}

		protected override Sample NextSample(IContext context)
		{
			var note = noteSource.GetNote(context);

			var delta = note.CurrentSample(context) - state.LatestNoteSample;

			if (delta < 0)
			{
				state.LatestSamplerSample = note.CurrentSample(context);
			}
			else
			{
				var pitch = note.Note?.ToPitch() ?? 0;

				state.LatestSamplerSample = state.LatestSamplerSample + delta * pitch;
			}

			var result = reader.Read((long)state.LatestSamplerSample);

			state.LatestNoteSample = note.CurrentSample(context);

			return result ?? 0;
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			var factory = Get<IWaveReaderFactory>(context);

			reader = factory.Create(sample);

			Initialize(context, noteSource, pitchSource);
		}

        public override void Dispose()
        {
            Dispose(noteSource, pitchSource);
        }
    }
}
