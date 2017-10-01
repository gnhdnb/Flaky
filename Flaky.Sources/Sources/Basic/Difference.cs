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

		protected override Sample NextSample(IContext context)
		{
			var aValue = a.Play(context);
			var bValue = b.Play(context);

			return new Sample
			{
				Left = aValue.Left - bValue.Left,
				Right = aValue.Right - bValue.Right,
			};
		}

		public override void Initialize(IContext context)
		{
			Initialize(context, a, b);
		}
        public override void Dispose()
        {
            Dispose(a, b);
        }
    }
}
