using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Note
	{
		private readonly int number;
		private readonly static double ratio = Math.Pow(2, 1d / 12d);
		private const float a4 = 440;
		private readonly float frequency;

		public Note(int number)
		{
			this.number = number;
			frequency = (float)(a4 * Math.Pow(ratio, number));
		}

		internal float ToFrequency()
		{
			return frequency;
		}

		public static implicit operator Note(int d)
		{
			return new Note(d);
		}
	}
}
