﻿using System;
using System.Collections.Generic;
using System.Linq;
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

		public static implicit operator Sample(PlayingNote n)
		{
			return new Sample { Value = n.note.ToFrequency() };
		}

		public float PlayTime(IContext context)
		{
			float sampleRate = 44100;

			return PlaySample(context) / sampleRate;
		}

		public long PlaySample(IContext context)
		{
			return context.Sample - startSample;
		}

		public Note Note
		{
			get { return note; }
		}
	}
}