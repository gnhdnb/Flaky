using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class DoublePendulum : Source
	{
		private Source l1Source;
		private Source l2Source;
		private Source m1Source;
		private Source m2Source;
		private Source gSource;

		private double theta1;
		private double theta1d;
		private double theta2;
		private double theta2d;

		private long sample = 0;

		public DoublePendulum()
		{
			theta1 = Math.PI / 2;
			theta2 = Math.PI / 2;
			m1Source = 1;
			m2Source = 1;
			l1Source = 5000;
			l2Source = 5000;
			gSource = 9.82;
		}

		public DoublePendulum(Source m1, Source m2, Source l1, Source l2, Source g)
		{
			theta1 = Math.PI / 2;
			theta2 = Math.PI / 2;
			m1Source = m1;
			m2Source = m2;
			l1Source = l1;
			l2Source = l2;
			gSource = g;
		}

		private void InternalStep(float delta, IContext context, float m1, float m2, float l1, float l2, float g)
		{
			double dt = delta;

			double theta1dd =
				(-g * (2 * m1 + m2) * Math.Sin(theta1)
				- m2 * g * Math.Sin(theta1 - 2 * theta2)
				- 2 * Math.Sin(theta1 - theta2) * m2 * (theta2d * theta2d * l2 + theta1d * theta1d * l1 * Math.Cos(theta1 - theta2)))
				/
				(l2 * (2 * m1 + m2 - m2 * Math.Cos(2 * theta1 - 2 * theta2)));

			double theta2dd =
				2 * Math.Sin(theta1 - theta2)
				* (theta1d * theta1d * l1 * (m1 + m2)
					+ g * (m1 + m2) * Math.Cos(theta1)
					+ theta2d * theta2d * l2 * m2 * Math.Cos(theta1 - theta2))
				/
				(l2 * (2 * m1 + m2 - m2 * Math.Cos(2 * theta1 - 2 * theta2)));

			if (double.IsNaN(theta1dd) || double.IsNaN(theta2dd))
			{
				theta1 = Math.PI / 2;
				theta2 = Math.PI / 2;
				theta1d = 0;
				theta2d = 0;
				return;
			}

			theta1d += theta1dd * dt;
			theta2d += theta2dd * dt;

			theta1 += theta1d * dt;
			theta2 += theta2d * dt;
		}

		public override Sample Play(IContext context)
		{
			var m1 = m1Source.Play(context).Value;
			var m2 = m2Source.Play(context).Value;
			var l1 = l1Source.Play(context).Value;
			var l2 = l2Source.Play(context).Value;
			var g = gSource.Play(context).Value;

			if(sample == 0)
				sample = context.Sample;

			var delta = context.Sample - sample;

			if (delta == 0)
				return new Sample { Value = 0 };

			sample = context.Sample;

			for(int i=0; i < 10; i++)
				InternalStep(((float)delta) / 10, context, m1, m2, l1, l2, g);

			return new Sample { Value = (float)((l1 * Math.Sin(theta1) + l2 * Math.Sin(theta2)) / (l1 + l2)) };
		}

		internal override void Initialize(IContext context)
		{
			Initialize(context, l1Source, l2Source, m1Source, m2Source, gSource);
		}
	}
}
