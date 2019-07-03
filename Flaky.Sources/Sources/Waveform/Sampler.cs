using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Sampler : Source, IPipingSource<NoteSource>
	{
		private readonly string sample;
		private INoteSource noteSource;
		private readonly float pitch;
		private IWaveReader reader;
		private State state;

		private class State
		{
			public long LatestNoteSample;
			public double LatestSamplerSample;
		}

		internal Sampler(string sample, float pitch, string id) : base(id)
		{
			this.sample = sample;
			this.pitch = pitch;
		}

		protected override Vector2 NextSample(IContext context)
		{
			var note = noteSource.GetNote(context);

			var delta = note.CurrentSample(context) - state.LatestNoteSample;

			if (delta < 0)
			{
				state.LatestSamplerSample = note.CurrentSample(context);
			}
			else
			{
				var notePitch = note.Note.IsSilent ? 0 : note.Note.ToPitch();

				state.LatestSamplerSample = state.LatestSamplerSample + delta * notePitch * pitch;
			}

			var result = reader.Read((long)state.LatestSamplerSample);

			state.LatestNoteSample = note.CurrentSample(context);

			return result ?? new Vector2(0, 0);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			var factory = Get<IWaveReaderFactory>(context);

			reader = factory.Create(context, sample);

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
