using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flaky
{
	public class Fourier : Source, IPipingSource
	{
		private Source input;
		private State state;
		private readonly float effect;

		private class State : IDisposable
		{
			public const int framesCount = 8192;
			private bool reverse = false;
			private int effect;
			public Thread worker;
			public BlockingCollection<Vector2[]> inputQueue = new BlockingCollection<Vector2[]>();
			public BlockingCollection<Vector2[]> outputQueue = new BlockingCollection<Vector2[]>();
			public Vector2[] latestOutputBuffer = new Vector2[framesCount];
			public Vector2[] outputBuffer = new Vector2[framesCount];
			public Vector2[] inputBuffer = new Vector2[framesCount];
			public Vector2[] secondInputBuffer = new Vector2[framesCount];
			public int currentFrame = 0;
			public float[] leftInputBuffer = new float[framesCount];
			public float[] rightInputBuffer = new float[framesCount];
			public float[] leftOutputBuffer = new float[framesCount];
			public float[] rightOutputBuffer = new float[framesCount];
			public Mdct leftForward = new Mdct(framesCount, true);
			public Mdct rightForward = new Mdct(framesCount, true);
			public Mdct leftBackward = new Mdct(framesCount, false);
			public Mdct rightBackward = new Mdct(framesCount, false);
			public CancellationTokenSource disposing = new CancellationTokenSource();
			public Sample latestSample;

			public State()
			{
				worker = new Thread(ProcessNextBatch);
				worker.Start();
				outputQueue.Add(new Vector2[framesCount]);
			}

			public void Initialize(float effect)
			{
				if (effect < -1)
					effect = -1;

				if (effect < 0)
				{
					this.reverse = true;
					effect = -effect;
				}

				if (effect > 1)
					effect = 1;

				this.effect = (int)Math.Floor((framesCount - 1) * effect);
			}

			private void ProcessNextBatch()
			{
				try
				{
					while (true)
					{
						var samples = inputQueue.Take(disposing.Token);
						ImportSamples(samples, leftInputBuffer, rightInputBuffer);
						leftForward.Forward(leftInputBuffer, leftOutputBuffer);
						rightForward.Forward(rightInputBuffer, rightOutputBuffer);

						var leftThreshold = ThresholdPower(leftOutputBuffer);
						var rightThreshold = ThresholdPower(rightOutputBuffer);

						for (int i = 0; i < framesCount; i++)
						{
							if (Power(leftOutputBuffer[i]) >= leftThreshold)
								leftOutputBuffer[i] = 0;

							if (Power(rightOutputBuffer[i]) >= rightThreshold)
								rightOutputBuffer[i] = 0;
						}

						leftBackward.Backward(leftOutputBuffer, leftInputBuffer);
						rightBackward.Backward(rightOutputBuffer, rightInputBuffer);
						ExportSamples(samples, leftInputBuffer, rightInputBuffer);
						outputQueue.Add(samples);
					}
				}
				catch (OperationCanceledException) { }
			}

			private float ThresholdPower(float[] buffer)
			{
				float[] orderedPower = buffer
					.Select(v => Power(v))
					.OrderBy(v => v)
					.ToArray();

				return orderedPower[framesCount - effect - 1];

				var average = orderedPower.Average();

				for (int i = 0; i < framesCount; i++)
				{
					if (orderedPower[i] > average)
						return orderedPower[i];
				}

				return orderedPower.Last();
			}

			private float Power(float value)
			{
				return reverse ? -Math.Abs(value) : Math.Abs(value);
			}

			private void ImportSamples(Vector2[] input, float[] leftBuffer, float[] rightBuffer)
			{
				for (int i = 0; i < framesCount; i++)
				{
					leftBuffer[i] = input[i].X * WindowFunction.KaiserBesselDerived8192.GetValue(i);
					rightBuffer[i] = input[i].Y * WindowFunction.KaiserBesselDerived8192.GetValue(i);
				}
			}

			private void ExportSamples(Vector2[] output, float[] leftBuffer, float[] rightBuffer)
			{
				for (int i = 0; i < framesCount; i++)
				{
					output[i].X = leftBuffer[i] * WindowFunction.KaiserBesselDerived8192.GetValue(i);
					output[i].Y = rightBuffer[i] * WindowFunction.KaiserBesselDerived8192.GetValue(i);
				}
			}

			public void Dispose()
			{
				disposing.Cancel();
				worker.Join();
			}
		}

		private readonly int oversampling = 1;

		internal Fourier(float effect, int oversampling, string id) : base(id)
		{
			if (oversampling < 1)
				oversampling = 1;

			if (oversampling > 8)
				oversampling = 8;

			this.oversampling = oversampling;

			if (effect < -1)
				effect = -1;

			if (effect > 1)
				effect = 1;

			this.effect = effect;
		}

		protected override Vector2 NextSample(IContext context)
		{
			var d = 1 / (float)oversampling;

			var inputSample = input.Play(context);

			Sample outputSample = 0;

			for(int step = 1; step <= oversampling; step++)
			{
				var interpolated = 
					(inputSample * step + state.latestSample * (oversampling - step))
					* d;

				outputSample += Step(interpolated);
			}

			state.latestSample = inputSample;

			return outputSample * d;
		}

		private Sample Step(Sample inputSample)
		{ 
			const int chunkSize = State.framesCount / 2;

			state.inputBuffer[chunkSize + state.currentFrame] = inputSample;
			state.secondInputBuffer[state.currentFrame] = inputSample;
			var outputSample = state.outputBuffer[state.currentFrame] + state.latestOutputBuffer[state.currentFrame + chunkSize];

			state.currentFrame++;

			if (state.currentFrame >= chunkSize)
			{
				var bufferToEnqueue = state.inputBuffer;
				state.inputBuffer = state.secondInputBuffer;
				state.secondInputBuffer = state.latestOutputBuffer;
				state.latestOutputBuffer = state.outputBuffer;
				state.outputBuffer = state.outputQueue.Take();
				state.inputQueue.Add(bufferToEnqueue);
				state.currentFrame = 0;
			}

			return outputSample;
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			state.Initialize(effect);
			Initialize(context, input);
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.input = mainSource;
		}

		public override void Dispose()
		{
			Dispose(input);
		}
	}
}
