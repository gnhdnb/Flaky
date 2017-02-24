using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Filter : Source
	{
		private Source source;
		private Source cutoff;

		private Sample lp;

		public Filter(Source source, Source cutoff)
		{
			this.source = source;
			this.cutoff = cutoff;
		}

		public override void Initialize(IContext context)
		{
			Initialize(context, source, cutoff);
		}

		public override Sample Play(IContext context)
		{
			var sample = source.Play(context);
			var cutoffValue = cutoff.Play(context).Value;

			if (cutoffValue < 0)
				cutoffValue = 0;

			if (cutoffValue > 1)
				cutoffValue = 1;

			var hp = sample - lp;
			lp = lp + hp * cutoffValue;

			return lp;
		}
	}
}
