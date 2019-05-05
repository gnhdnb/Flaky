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
	public abstract class FrequencyDomainOperator : Source, IPipingSource
	{
		private Source input;
		private State state;

		private class State : IDisposable
		{
			public const int framesCount = 8192;
			public Thread worker;
			public BlockingCollection<(Vector2[], float)> inputQueue = new BlockingCollection<(Vector2[], float)>();
			public BlockingCollection<(Vector2[], float)> outputQueue = new BlockingCollection<(Vector2[], float)>();
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
			public Vector2 latestSample;

			public Action<float[], float[], float> Processor { get; set; }

			public State()
			{
				worker = new Thread(ProcessNextBatch);
				worker.Start();
				outputQueue.Add((new Vector2[framesCount], 0));
			}

			private void ProcessNextBatch()
			{
				try
				{
					while (true)
					{
						var samples = inputQueue.Take(disposing.Token);
						disposing.Token.ThrowIfCancellationRequested();

						ImportSamples(samples.Item1, leftInputBuffer, rightInputBuffer);
						leftForward.Forward(leftInputBuffer, leftOutputBuffer);
						rightForward.Forward(rightInputBuffer, rightOutputBuffer);

						Processor?.Invoke(leftOutputBuffer, rightOutputBuffer, samples.Item2);

						leftBackward.Backward(leftOutputBuffer, leftInputBuffer);
						rightBackward.Backward(rightOutputBuffer, rightInputBuffer);
						ExportSamples(samples.Item1, leftInputBuffer, rightInputBuffer);
						outputQueue.Add(samples);
					}
				}
				catch (OperationCanceledException) { }
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

		internal FrequencyDomainOperator(int oversampling, string id) : base(id)
		{
			if (oversampling < 1)
				oversampling = 1;

			if (oversampling > 8)
				oversampling = 8;

			this.oversampling = oversampling;
		}

		protected sealed override Vector2 NextSample(IContext context)
		{
			var d = 1 / (float)oversampling;

			var inputSample = input.Play(context);
			var effect = GetEffect(context);

			Vector2 outputSample = new Vector2(0);

			for(int step = 1; step <= oversampling; step++)
			{
				var interpolated = 
					(inputSample * step + state.latestSample * (oversampling - step))
					* d;

				outputSample += Step(interpolated, effect);
			}

			state.latestSample = inputSample;

			return outputSample * d;
		}

		protected abstract float GetEffect(IContext context);

		protected abstract void Processor(float[] left, float[] right, float effect);

		private Vector2 Step(Vector2 inputSample, float effect)
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
				state.outputBuffer = state.outputQueue.Take().Item1;
				state.inputQueue.Add((bufferToEnqueue, effect));
				state.currentFrame = 0;
			}

			return outputSample;
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			state.Processor = Processor;
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
