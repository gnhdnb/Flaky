using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Met : Source
	{
		public override void Initialize(IContext context)
		{
		}

        public override void Dispose()
        { 
        }

		protected override Sample NextSample(IContext context)
		{
			if (context.MetronomeTick) {
				if (context.Beat % 4 == 0)
					return 0.5f;
				else
					return 0.2f;
			}

			return 0f;
		}
	}
}
