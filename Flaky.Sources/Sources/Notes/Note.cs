using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Note : IEquatable<Note>
	{
		private readonly int number;
		private readonly static double ratio = Math.Pow(2, 1d / 12d);
		public const float BaselineFrequency = 440;
		private readonly float frequency;
		private readonly float pitch;

		public Note(int number)
		{
			this.number = number;
			pitch = (float)Math.Pow(ratio, number);
			frequency = (float)(BaselineFrequency * Math.Pow(ratio, number));
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
			if (other == null)
				return false;

			return this.number == other.number;
		}

		internal int Number
		{
			get { return number; }
		}

		public static implicit operator Note(int d)
		{
			return new Note(d);
		}
	}
}
