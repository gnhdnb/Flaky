using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

		protected override Vector2 NextSample(IContext context)
		{
			var aValue = a.Play(context);
			var bValue = b.Play(context);

			return aValue - bValue;
		}

		protected override void Initialize(IContext context)
		{
			Initialize(context, a, b);
		}
        public override void Dispose()
        {
            Dispose(a, b);
        }
    }
}
