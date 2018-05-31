using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Sampler : Source, IPipingSource<NoteSource>
	{
		private readonly string sample;
		private NoteSource noteSource;
		private readonly Source pitchSource;
		private IWaveReader reader;
		private State state;

		private class State
		{
			public long LatestNoteSample;
			public double LatestSamplerSample;
		}

		internal Sampler(string sample, string id) : base(id)
		{
			this.sample = sample;
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

		protected override void Initialize(IContext context)
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

		void IPipingSource<NoteSource>.SetMainSource(NoteSource mainSource)
		{
			this.noteSource = mainSource;
		}
	}
}
