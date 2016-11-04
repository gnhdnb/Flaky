using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class AD : Source
	{
		private NoteSource source;
		private Source attack;
		private Source decay;

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

		public override Sample Play(IContext context)
		{
			var note = source.GetNote(context);

			var attackValue = attack.Play(context).Value;
			var decayValue = decay.Play(context).Value;

			if (attackValue < 0)
				return new Sample { Value = 0 };

			if (decayValue < 0)
				return new Sample { Value = 0 };

			var attackLeft = attackValue - note.CurrentTime(context);
			var decayLeft = attackValue + decayValue - note.CurrentTime(context);

			if (attackLeft >= 0)
			{
				return new Sample { Value = (attackValue - attackLeft) / attackValue };
			}

			if (decayLeft >= 0)
			{
				return new Sample { Value = decayLeft / decayValue };
			}

			return new Sample { Value = 0 };
		}

		internal override void Initialize(IContext context)
		{
			Initialize(context, source, attack, decay);
		}
	}
}
