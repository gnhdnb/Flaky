using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flaky
{
	public class Fourier : Source, IPipingSource
	{
		private const int framesCount = 8192;
		private Thread worker;
		private Source input;
		private BlockingCollection<Sample[]> inputQueue = new BlockingCollection<Sample[]>();
		private BlockingCollection<Sample[]> outputQueue = new BlockingCollection<Sample[]>();
		private Sample[] latestOutputBuffer = new Sample[framesCount];
		private Sample[] outputBuffer = new Sample[framesCount];
		private Sample[] inputBuffer = new Sample[framesCount];
		private Sample[] secondInputBuffer = new Sample[framesCount];
		private int currentFrame = 0;
		private float[] leftInputBuffer = new float[framesCount];
		private float[] rightInputBuffer = new float[framesCount];
		private float[] leftOutputBuffer = new float[framesCount];
		private float[] rightOutputBuffer = new float[framesCount];
		private Mdct leftForward = new Mdct(framesCount, true);
		private Mdct rightForward = new Mdct(framesCount, true);
		private Mdct leftBackward = new Mdct(framesCount, false);
		private Mdct rightBackward = new Mdct(framesCount, false);
		private CancellationTokenSource disposing = new CancellationTokenSource();
		private readonly int effect;

		internal Fourier(float effect)
		{
			if (effect < 0)
				effect = 0;

			if (effect > 1)
				effect = 1;

			this.effect = (int)Math.Floor((framesCount - 1) * effect);
			outputQueue.Add(new Sample[framesCount]);
		}

		protected override Sample NextSample(IContext context)
		{
			var chunkSize = framesCount / 2;

			var inputSample = input.Play(context);
			inputBuffer[chunkSize + currentFrame] = inputSample;
			secondInputBuffer[currentFrame] = inputSample;
			var outputSample = outputBuffer[currentFrame] + latestOutputBuffer[currentFrame + chunkSize];

			currentFrame++;

			if(currentFrame >= chunkSize)
			{
				var bufferToEnqueue = inputBuffer;
				inputBuffer = secondInputBuffer;
				secondInputBuffer = latestOutputBuffer;
				latestOutputBuffer = outputBuffer;
				outputBuffer = outputQueue.Take();
				inputQueue.Add(bufferToEnqueue);
				currentFrame = 0;
			}

			return outputSample;
		}

		public override void Initialize(IContext context)
		{
			worker = new Thread(ProcessNextBatch);
			worker.Start();
			Initialize(context, input);
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
						if(Power(leftOutputBuffer[i]) > leftThreshold)
							leftOutputBuffer[i] = 0;

						if(Power(rightOutputBuffer[i]) > rightThreshold)
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
			var orderedPower = buffer
				.Select(v => Power(v))
				.OrderBy(v => v)
				.ToArray();

			return orderedPower[framesCount - effect - 1];

			var average = orderedPower.Average();

			for(int i = 0; i < framesCount; i++)
			{
				if (orderedPower[i] > average)
					return orderedPower[i];
			}

			return orderedPower.Last();
		}

		private float Power(float value)
		{
			return Math.Abs(value);
		}

		private void ImportSamples(Sample[] input, float[] leftBuffer, float[] rightBuffer)
		{
			for (int i = 0; i < framesCount; i++)
			{
				leftBuffer[i] = input[i].Left * WindowFunction.KaiserBesselDerived8192.GetValue(i);
				rightBuffer[i] = input[i].Right * WindowFunction.KaiserBesselDerived8192.GetValue(i);
			}
		}

		private void ExportSamples(Sample[] output, float[] leftBuffer, float[] rightBuffer)
		{
			for (int i = 0; i < framesCount; i++)
			{
				output[i].Left = leftBuffer[i] * WindowFunction.KaiserBesselDerived8192.GetValue(i);
				output[i].Right = rightBuffer[i] * WindowFunction.KaiserBesselDerived8192.GetValue(i);
			}
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.input = mainSource;
		}

		public override void Dispose()
		{
			disposing.Cancel();
			worker.Join();
			Dispose(input);
		}
	}
}
