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
	}
}
