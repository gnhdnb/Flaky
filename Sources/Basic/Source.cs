using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public abstract class Source
	{
		public abstract Sample Play(IContext context);


		public static implicit operator Source(float d)
		{
			return new Constant(d);
		}

		public static implicit operator Source(int d)
		{
			return new Constant(d);
		}

		public static implicit operator Source(double d)
		{
			return new Constant((float)d);
		}

		public static Source operator +(Source a, Source b)
		{
			return new Sum(a, b);
		}

		public static Source operator -(Source a, Source b)
		{
			return new Difference(a, b);
		}

		public static Source operator *(Source a, Source b)
		{
			return new Multiply(a, b);
		}
	}
}
