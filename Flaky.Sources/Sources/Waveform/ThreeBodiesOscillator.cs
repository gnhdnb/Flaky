using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class ThreeBodiesOscillator : Source
	{
		private List<Body> bodies = new List<Body>();

		public override void Dispose()
		{
			
		}

		public override void Initialize(IContext context)
		{
			bodies.Add(
					new Body
					{
						Mass = 1,
						Position = new Vector(0, 0.3),
						Velocity = new Vector(0, 0)
					}
				);

			bodies.Add(
					new Body
					{
						Mass = 0.5,
						Position = new Vector(-0.1, -0.1),
						Velocity = new Vector(0, 0)
					}
				);

			bodies.Add(
					new Body
					{
						Mass = 0.7,
						Position = new Vector(0.1, -0.1),
						Velocity = new Vector(0, 0)
					}
				);
		}

		public override Sample Play(IContext context)
		{
			for(int i = 0; i < 10; i++)
			{
				Step();
			}

			return new Sample
			{
				Left = (float)bodies[0].Position.X,
				Right = (float)bodies[0].Position.Y
			};
		}

		private void Step()
		{
			const double k = 0.00001;

			foreach(var body1 in bodies)
				foreach (var body2 in bodies)
					if(body1 != body2)
					{
						var force1 = 10 * k * body1.Mass * body2.Mass / (body2.Position - body1.Position).LengthSquare;

						if (double.IsNaN(force1))
							force1 = 0;

						body2.Velocity += force1 * (body2.Position - body1.Position).Normalized;
						
					}

			foreach (var body in bodies)
			{
				var distanceFromCenter = (body.Position - new Vector(0, 0)).LengthSquare;

				/*var force2 = distanceFromCenter > 1 ? 
					k * distanceFromCenter
					: 0;*/

				var force2 = k * distanceFromCenter;

				if (double.IsNaN(force2))
					force2 = 0;

				body.Velocity += force2 * (new Vector(0, 0) - body.Position).Normalized;

				body.Position += body.Velocity;
			}
		}

		private class Body
		{
			public double Mass;
			public Vector Position;
			public Vector Velocity;
		}

		private struct Vector
		{
			public Vector(double x, double y)
			{
				X = x;
				Y = y;
			}

			public double X;
			public double Y;

			public double LengthSquare
			{
				get
				{
					return X * X + Y * Y;
				}
			}

			public double Length
			{
				get
				{
					return Math.Sqrt(LengthSquare);
				}
			}

			public Vector Normalized
			{
				get
				{
					return new Vector(X / Length, Y / Length);
				}
			}

			public static Vector operator +(Vector a, Vector b)
			{
				return new Vector(a.X + b.X, a.Y + b.Y);
			}

			public static Vector operator -(Vector a, Vector b)
			{
				return new Vector(a.X - b.X, a.Y - b.Y);
			}

			public static Vector operator *(Vector a, double b)
			{
				return new Vector(a.X * b, a.Y * b);
			}

			public static Vector operator *(double b, Vector a)
			{
				return a * b;
			}
		}
	}
}
