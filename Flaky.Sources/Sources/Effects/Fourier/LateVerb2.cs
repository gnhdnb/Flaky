using System;
using System.Numerics;

namespace Flaky
{
	public class LateVerb2 : Source, IPipingSource
	{
		private Source input;
		private Source effect;
		private State state;

		private class State
		{
			private const int hop = 256;
			private const int bufferSize = 1024;
			private const float effect = 0.01f;

			private int hopCounter = 0;
			private int bufferCounter = 0;

			private Vector2[] buffer = new Vector2[bufferSize];
			private Vector2[] fftBuffer = new Vector2[bufferSize];
			private float[] amps = new float[bufferSize];
			private float[] activeAmps = new float[bufferSize];
			private double[] phase = new double[bufferSize];

			public Vector2 NextSample(Vector2 input)
			{
				buffer[bufferCounter] = new Vector2(input.X, 0);

				bufferCounter += 1;
				bufferCounter %= bufferSize;

				if (hopCounter == 0)
				{
					for (int i = 0; i < bufferSize; i++)
						fftBuffer[i] = buffer[i + bufferCounter % bufferSize];

					FFT.InplaceFFT(fftBuffer);

					for (int i = 0; i < bufferSize; i++)
						amps[i] = fftBuffer[i].Length();
				}

				hopCounter += 1;
				hopCounter %= hop;

				float output = 0;

				for (int i = 0; i < bufferSize; i++)
				{
					activeAmps[i] = amps[i] * effect
						+ activeAmps[i] * (1 - effect);

					double term = 2 * Math.PI * i / bufferSize;

					phase[i] += term;

					phase[i] %= 2 * Math.PI;

					output += activeAmps[i] * (float)Math.Sin(phase[i]);
				}


				return new Vector2(output, output);
			}
		}

		internal LateVerb2(Source effect, string id): base(id)
		{
			this.effect = effect;
		}

		protected override Vector2 NextSample(IContext context)
		{
			var inputValue = input.Play(context);

			return state.NextSample(inputValue);
		}

		public override void Dispose()
		{
			Dispose(input, effect);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);

			Initialize(context, input, effect);
		}

		public void SetMainSource(Source mainSource)
		{
			this.input = mainSource;
		}
	}
}
