using Extreme.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Flaky
{
	public class PinkNoiseBasedMix : Source
	{
		private Source[] sources;
		private States states;
		private Vector2[] values;

		public PinkNoiseBasedMix(string id, params Source[] sources) : base(id)
		{
			this.sources = sources;
			values = new Vector2[sources.Length];
		}

		private class States
		{
			public List<State> states = new List<State>();
		}

		private class State
		{
			public const int framesCount = 8192;
			public const int bandsCount = 4096;
			public Mdct forward = new Mdct(framesCount, true);
			public int currentFrame = 0;
			public float[] buffer = new float[framesCount];
			public float[] freqBuffer = new float[framesCount];
			public float[] decisionBuffer = new float[bandsCount];
			public double[] masterSpectrum = new double[bandsCount];
			public float desiredVolume = 0;
			public float volume = 0;
			public bool skip = false;

			public State()
			{
				for (int i = 0; i < bandsCount; i++)
				{
					masterSpectrum[i] = 1 / ((double)i + 1);
				}

				for (int i = 80; i < 120; i++)
				{
					masterSpectrum[i] = masterSpectrum[i] * 2;
				}
			}
		}

		public override void Dispose()
		{
			
		}

		protected override void Initialize(IContext context)
		{
			states = GetOrCreate<States>(context);
			Initialize(context, sources);

			while (states.states.Count < sources.Length)
				states.states.Add(new State());
		}

		protected override Vector2 NextSample(IContext context)
		{
			for (int i = 0; i < sources.Length; i++)
			{
				var state = states.states[i];
				values[i] = sources[i].Play(context);
				state.buffer[states.states[0].currentFrame] = values[i].X;
			}

			states.states[0].currentFrame++;

			if (states.states[0].currentFrame >= State.framesCount)
			{
				for (int i = 0; i < sources.Count(); i++)
				{
					var state = states.states[i];

					state.forward.Forward(state.buffer, state.freqBuffer);

					float sum = 0;

					for (int j = 0; j < State.bandsCount; j++)
					{
						state.decisionBuffer[j] *= 0.99f;

						state.decisionBuffer[j] =
							Math.Max(state.decisionBuffer[j], Math.Abs(state.freqBuffer[j]));

						sum += Math.Abs(state.freqBuffer[j]);
					}

					state.skip = sum < 0.01;
				}

				var master = Extreme.Mathematics.Vector.Create(states.states[0].masterSpectrum);

				foreach (var state in states.states.Where(s => !s.skip))
				{
					var matrix = Matrix.Create(
						State.bandsCount,
						1,
						state.decisionBuffer
							.Select(v => Math.Abs((double)v))
							.ToArray(),
						MatrixElementOrder.ColumnMajor);

					var x = matrix.LeastSquaresSolve(master);

					state.desiredVolume = (float)x[0];
				}

				states.states[0].currentFrame = 0;
			}

			Vector2 result = Vector2.Zero;

			for(int i = 0; i < sources.Count(); i++)
			{
				/*if (states.states[i].desiredVolume > 1)
					states.states[i].desiredVolume = 1;*/

				states.states[i].volume += 
					(states.states[i].desiredVolume - states.states[i].volume)
						* 0.00001f;

				result += states.states[i].volume * values[i];
			}

			return result;
		}
	}
}
