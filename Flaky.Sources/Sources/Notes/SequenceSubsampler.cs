using Redzen.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class SequenceSubsampler : NoteSource, IPipingSource<NoteSource>
	{
		private NoteSource mainSource;
		private Source probability;
		private State state;
		private XorShiftRandom random = new XorShiftRandom();

		public SequenceSubsampler(Source probability, string id) : base(id)
		{
			this.probability = probability;
		}

		private class State
		{
			public long latestNoteStartSample = -1;
			public bool shouldPlay = false;
		}

		public override void Dispose()
		{
			Dispose(mainSource, probability);
		}

		public override PlayingNote GetNote(IContext context)
		{
			var note = mainSource.GetNote(context);
			var probabilityValue = probability.Play(context).Value;

			if (note.StartSample != state.latestNoteStartSample)
			{
				state.latestNoteStartSample = note.StartSample;

				state.shouldPlay = random.NextDouble() < probabilityValue;
			}

			if (state.shouldPlay)
				return note;
			else
				return new PlayingNote(null, state.latestNoteStartSample);
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			Initialize(context, mainSource, probability);
		}

		void IPipingSource<NoteSource>.SetMainSource(NoteSource mainSource)
		{
			this.mainSource = mainSource;
		}
	}
}
