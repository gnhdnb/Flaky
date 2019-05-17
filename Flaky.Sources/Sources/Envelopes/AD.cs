using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class AD : Source
	{
		private INoteSource source;
		private Source attack;
		private Source decay;
		private PlayingNote currentNote;
		private State state;

		private class State
		{
			public float value;
		}

		public AD(NoteSource source, Source decay)
		{
			this.source = source;
			this.attack = 0;
			this.decay = decay;
		}

		public AD(NoteSource source, Source attack, Source decay)
		{
			this.source = source;
			this.attack = attack;
			this.decay = decay;
		}

		public AD(NoteSource source, Source attack, Source decay, string id) : base(id)
		{
			this.source = source;
			this.attack = attack;
			this.decay = decay;
		}

		protected override Vector2 NextSample(IContext context)
		{
			var note = source.GetNote(context);

			if (currentNote.Note == null || !note.IsSilent)
				currentNote = note;

			var attackValue = attack.Play(context).X;
			var decayValue = decay.Play(context).X;

			if (attackValue < 0)
				return new Vector2(0, 0);

			if (decayValue < 0)
				return new Vector2(0, 0);

			if (currentNote.IsSilent)
				return new Vector2(0, 0);

			var attackLeft = attackValue - currentNote.CurrentTime(context);
			var decayLeft = attackValue + decayValue - currentNote.CurrentTime(context);

			if (attackLeft > 0)
			{
				var output = (attackValue - attackLeft) / attackValue;

				if (output < state.value)
				{
					return new Vector2(state.value, state.value);
				}
				else
				{
					state.value = output;

					return new Vector2(output, output);
				}
			}

			if (decayLeft > 0)
			{
				var output = decayLeft / decayValue;

				state.value = output;

				return new Vector2(output, output);
			}

			return new Vector2(0, 0);
		}

		protected override void Initialize(IContext context)
		{
			Initialize(context, source, attack, decay);

			state = GetOrCreate<State>(context);
		}

		public override void Dispose()
		{
			Dispose(source, attack, decay);
		}
	}
}
