using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public struct Sample
	{
		public float Left;
		public float Right;

		public float Value
		{
			get
			{
				return (Left + Right) / 2;
			}
			set
			{
				Left = value;
				Right = value;
			}
		}

		public static Sample operator+ (Sample a, Sample b)
		{
			return new Sample
			{
				Left = a.Left + b.Left,
				Right = a.Right + b.Right
			};
		}

		public static Sample operator -(Sample a, Sample b)
		{
			return new Sample
			{
				Left = a.Left - b.Left,
				Right = a.Right - b.Right
			};
		}

		public static Sample operator /(Sample a, float b)
		{
			return new Sample
			{
				Left = a.Left / b,
				Right = a.Right / b
			};
		}
	}
}
