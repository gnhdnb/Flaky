using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Met : Source
	{
		protected override void Initialize(IContext context)
		{
		}

        public override void Dispose()
        { 
        }

		protected override Vector2 NextSample(IContext context)
		{
			if (context.MetronomeTick) {
				if (context.Beat % 4 == 0)
					return new Vector2(0.5f, 0.5f);
				else
					return new Vector2(0.2f, 0.2f);
			}

			return new Vector2(0f, 0f);
		}
	}
}
