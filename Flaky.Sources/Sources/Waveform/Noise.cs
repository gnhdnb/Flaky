using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Noise : Source
	{
		private static Random random = new Random();

		protected override void Initialize(IContext context) { }

		public override void Dispose() { }

		protected override Sample NextSample(IContext context)
		{
			return new Sample
			{
				Left = (float)random.NextDouble() * 2 - 1,
				Right = (float)random.NextDouble() * 2 - 1,
			};
		}
	}
}
