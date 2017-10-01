using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal class Hold : Source
	{
		public Sample Sample { get; set; }

		public override void Initialize(IContext context) { }

		protected override Sample NextSample(IContext context)
		{
			return Sample;
		}

        public override void Dispose() { }
    }
}
