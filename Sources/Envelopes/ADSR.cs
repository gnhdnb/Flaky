using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class ADSR : Source
	{
		private NoteSource source;
		private Source decay;

		public ADSR(NoteSource source, Source decay)
		{
			this.source = source;
			this.decay = decay;
		}

		public override Sample Play(IContext context)
		{
			var note = source.GetNote(context);

			var decayValue = decay.Play(context).Value;

			if (decayValue <= 0)
				return new Sample { Value = 0 };

			var secondsLeft = decayValue - note.PlayTime(context);

			if (secondsLeft <= 0)
				return new Sample { Value = 0 };

			return new Sample { Value = secondsLeft / decayValue };
		}
	}
}
