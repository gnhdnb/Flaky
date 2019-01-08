using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class SequenceAggregator : PolyphonicNoteSource, IPipingSource<NoteSource>
	{
		private NoteSource mainSource;
		private SequenceAggregatorVoice[] voices;
		private bool initialized;
		private State state;

		private class State
		{
			public PlayingNote[] chord = new PlayingNote[0];
			public List<Note> notes = new List<Note>();
		}

		protected override NoteSource[] Sources => voices;

		public SequenceAggregator(int voices, string id) : base(id)
		{
			this.voices =
				Enumerable
				.Range(0, voices)
				.Select(i => new SequenceAggregatorVoice(this, i))
				.ToArray();
		}

		public override void Dispose()
		{
			Dispose(mainSource);
		}

		public override PlayingNote[] GetNotes(IContext context)
		{
			return state.chord;
		}

		private PlayingNote GetNote(IContext context, int voiceNumber)
		{
			if(voiceNumber == 0)
			{
				var playingNote = mainSource.GetNote(context);

				if (playingNote.CurrentSample(context) == 0)
				{
					state.notes.Add(playingNote.Note);

					if (state.notes.Count >= voices.Length)
					{
						state.chord = state.notes
							.Where(n => n != null)
							.Distinct()
							.Select(n => new PlayingNote(n, context.Sample))
							.ToArray();

						state.notes.Clear();
					}
				}
			}

			if (voiceNumber >= state.chord.Length)
				return new PlayingNote(null, context.Sample);

			return state.chord[voiceNumber];
		}

		protected override void Initialize(IContext context)
		{
			if (!initialized)
			{
				state = GetOrCreate<State>(context);
				Initialize(context, mainSource);
				initialized = true;
			}
		}

		void IPipingSource<NoteSource>.SetMainSource(NoteSource mainSource)
		{
			this.mainSource = mainSource;
		}

		public class SequenceAggregatorVoice : NoteSource
		{
			private readonly SequenceAggregator aggregator;
			private readonly int voiceNumber;

			internal SequenceAggregatorVoice(
				SequenceAggregator aggregator,
				int voiceNumber)
			{
				this.aggregator = aggregator;
				this.voiceNumber = voiceNumber;
			}

			public override PlayingNote GetNote(IContext context)
			{
				return aggregator.GetNote(context, voiceNumber);
			}

			protected override void Initialize(IContext context)
			{
				aggregator.Initialize(context);
			}

			public override void Dispose()
			{
				aggregator.Dispose();
			}
		}
	}
}
