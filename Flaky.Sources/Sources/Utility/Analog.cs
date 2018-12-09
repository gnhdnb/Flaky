using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal class Analog : Source
	{
		private class State
		{
			private Effect left = new Effect();
			private Effect right = new Effect();

			public Vector2 NextSample(Vector2 input)
			{
				return new Vector2 {
					X = (float)left.NextSample(input.X),
					Y = (float)right.NextSample(input.Y),
				};
			}

			private class Effect
			{
				const int preeffectBufferSize = 15;
				private double velocity = 0;
				private double velocity2 = 0;
				private double current = 0;
				private double[] preeffectBuffer = new double[preeffectBufferSize];
				private int preeffectBufferCounter = 0;
				private double preeffectAverage = 0;
				private double hpIntegrator = 0;
				private double lpIntegrator = 0;

				public double NextSample(double input)
				{
					var tunedSample = Drive(input * 8) * 0.25f * 0.07f;

					var currentSample = preeffectBuffer[preeffectBufferCounter];

					preeffectAverage += tunedSample / preeffectBufferSize;
					preeffectAverage -= currentSample / preeffectBufferSize;
					preeffectBuffer[preeffectBufferCounter] = tunedSample;
					preeffectBufferCounter++;
					if (preeffectBufferCounter >= preeffectBufferSize)
						preeffectBufferCounter = 0;

					var acc = (preeffectAverage - currentSample);

					velocity += acc * acc * acc * 30;
					velocity = velocity * 0.9f;

					velocity2 = velocity2 * 0.4f;
					velocity2 += (currentSample - current) * 2;

					current += velocity + velocity2;

					current = Drive(current * 20) / 20;

					hpIntegrator += (current - hpIntegrator) / (400 * (1 - Math.Abs(hpIntegrator * 20)));

					lpIntegrator += (current - hpIntegrator - lpIntegrator) /
						(1.5f + Math.Abs(lpIntegrator * 30) * Math.Abs(lpIntegrator * 30));

					return lpIntegrator * 10;
				}

				private double Drive(double value)
				{
					return Math.Sign(value) * (1 - (float)Math.Pow(Math.E, -Math.Abs(value)));
				}
			}
		}

		private State state;
		private Source input;

		public Analog(Source input, string id) : base(id)
		{
			this.input = input;
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			Initialize(context, input);
		}

		protected override Vector2 NextSample(IContext context)
		{
			return state.NextSample(input.Play(context));
		}

		public override void Dispose()
		{
			Dispose(input);
		}
	}
}
