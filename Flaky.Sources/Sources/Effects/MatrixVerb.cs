using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class MatrixVerb : Source, IPipingSource
	{
		private const int sizex = 122;
		private const int sizey = 4;
		private float[,] value = new float[sizex, sizey];
		private float[,] velocity = new float[sizex, sizey];
		private Source source;
		private Source viscosity;

		internal MatrixVerb()
		{
			this.viscosity = 30.0f;
		}

		internal MatrixVerb(Source viscosity)
		{
			this.viscosity = viscosity;
		}

		public override void Initialize(IContext context)
		{
			source.Initialize(context);
			viscosity.Initialize(context);
		}

		public override void Dispose()
        {
            Dispose(source, viscosity);
        }

		protected override Vector2 NextSample(IContext context)
		{
			var viscosityValue = viscosity.Play(context);

			var sample = source.Play(context);
			value[1, 1] = sample.X;
			value[1, 2] = sample.Y;

			for (int x = 1; x < sizex - 1; x++)
			for (int y = 1; y < sizey - 1; y++)
			{
				var delta = (
					(value[x - 1, y] + value[x + 1, y] + value[x, y - 1] + value[x, y + 1]) / 4
					- value[x, y]);

					velocity[x, y] += delta / viscosityValue.X;

					velocity[x, y] = velocity[x, y] / 1.0004f;
				}

			for (int x = 1; x < sizex - 1; x++)
			for (int y = 1; y < sizey - 1; y++)
			{
				value[x, y] += velocity[x, y];
			}

			return new Vector2
			{
				X = value[2, 1],
				Y = value[2, 2]
			};
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}
	}
}
