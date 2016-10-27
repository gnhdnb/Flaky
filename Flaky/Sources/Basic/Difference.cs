using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Difference : Source
	{
		private readonly Source a;
		private readonly Source b;

		internal Difference(Source a, Source b)
		{
			this.a = a;
			this.b = b;
		}

		public override Sample Play(IContext context)
		{
			return new Sample { Value = a.Play(context).Value - b.Play(context).Value };
		}

		internal override void Initialize(IContext context)
		{
			Initialize(context, a, b);
		}
	}
}
