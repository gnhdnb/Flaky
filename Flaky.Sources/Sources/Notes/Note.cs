using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public struct Note : IEquatable<Note>
	{
		private readonly int number;
		private readonly static double ratio = Math.Pow(2, 1d / 12d);
		public const float BaselineFrequency = 440;
		private readonly float frequency;
		private readonly float pitch;
		private readonly bool isNotSilent;

		public Note(int number, bool isSilent = false)
		{
			this.isNotSilent = !isSilent;
			this.number = isSilent ? number : 0;
			pitch = isSilent ? (float)Math.Pow(ratio, number) : 0;
			frequency = isSilent ? (float)(BaselineFrequency * Math.Pow(ratio, number)) : 0;
		}

		internal float ToFrequency()
		{
			return frequency;
		}

		internal float ToPitch()
		{
			return pitch;
		}

		public bool Equals(Note other)
		{
			if (other.IsSilent && IsSilent)
				return true;

			return this.number == other.number;
		}

		public static bool operator ==(Note a, Note b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Note a, Note b)
		{
			return !a.Equals(b);
		}

		internal int Number
		{
			get { return number; }
		}

		internal bool IsSilent
		{
			get { return !isNotSilent; }
		}

		public static implicit operator Note(int d)
		{
			return new Note(d);
		}

		public static Note Silent
		{
			get { return new Note(0, true); }
		}
	}
}
