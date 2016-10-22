using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Constant : Source
	{
		private readonly float value;

		internal Constant(float value)
		{
			this.value = value;
		}

		public override Sample Play(IContext context)
		{
			return new Sample { Value = value };
		}
	}
}
