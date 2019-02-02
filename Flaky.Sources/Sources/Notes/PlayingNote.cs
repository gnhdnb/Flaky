using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public struct PlayingNote
	{
		private readonly Note note;
		private readonly long startSample;

		public PlayingNote(Note note, long startSample)
		{
			this.note = note;
			this.startSample = startSample;
		}

		public static implicit operator Vector2(PlayingNote n)
		{
			var freq = n.note?.ToFrequency() ?? 0;
			return new Vector2(freq, freq);
		}

		public bool IsSilent { get { return note == null; } }

		public float CurrentTime(IContext context)
		{
			return CurrentSample(context) / (float)context.SampleRate;
		}

		public long CurrentSample(IContext context)
		{
			return context.Sample - startSample;
		}

		public long StartSample { get { return startSample; } }

		public Note Note
		{
			get { return note; }
		}

		public static bool operator==(PlayingNote a, PlayingNote b)
		{
			return a.note == b.note && a.startSample == b.startSample;
		}

		public static bool operator!=(PlayingNote a, PlayingNote b)
		{
			return !(a == b);
		}
	}
}
