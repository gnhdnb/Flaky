using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Noise : Source
	{
		private static Random random = new Random();

		public override void Initialize(IContext context) { }

		public override void Dispose() { }

		protected override Vector2 NextSample(IContext context)
		{
			return new Vector2
			{
				X = (float)random.NextDouble() * 2 - 1,
				Y = (float)random.NextDouble() * 2 - 1,
			};
		}
	}
}
