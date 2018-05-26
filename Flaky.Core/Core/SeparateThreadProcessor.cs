using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flaky
{
	internal class SeparateThreadProcessor : IExternalProcessor
	{
		private IFlakySource source;
		private ContextController controller;
		private ContextController externalController;
		private const int readBuffersCount = 3;
		private const int bufferSize = 6615;

		private long topReadingSample = 0;
		private bool isStopping = false;
		private bool initialized = false;

		private BufferCollection buffers = new BufferCollection();
		private ManualResetEvent nextBufferNeeded = new ManualResetEvent(true);
		private ManualResetEvent nextBufferReady = new ManualResetEvent(true);

		private class Buffer
		{
			public Sample[] samples = new Sample[bufferSize];
		}

		private class BufferCollection
		{
			public long offset;
			public Buffer[] readBuffer = new Buffer[readBuffersCount];
			public Buffer writeBuffer = new Buffer();

			public BufferCollection()
			{
				for (int i = 0; i < readBuffersCount; i++)
					this.readBuffer[i] = new Buffer();
			}

			public BufferCollection(BufferCollection other)
			{
				this.offset = other.offset;
				this.readBuffer = other.readBuffer.ToArray();
				this.writeBuffer = other.writeBuffer;
			}
		}

		private Thread worker;

		public SeparateThreadProcessor(
			IFlakySource source, 
			ContextController contextController)
		{
			this.source = source;
			this.externalController = contextController;
		}

		public Sample Play(IContext context)
		{
			if (!initialized)
				Initialize();

			WaitForBuffer(context);

			return buffers.readBuffer[(context.Sample - buffers.offset) / bufferSize]
				.samples[(context.Sample - buffers.offset) % bufferSize];
		}

		private void Initialize()
		{
			controller = externalController.Clone();

			buffers.offset = controller.Sample - readBuffersCount * bufferSize;

			worker = new Thread(Run);
			worker.Priority = ThreadPriority.Highest;

			worker.Start();

			initialized = true;
		}

		private void Run()
		{
			while (true)
			{
				while(topReadingSample < buffers.offset + bufferSize * readBuffersCount)
				{
					nextBufferNeeded.WaitOne(10);
					nextBufferNeeded.Reset();

					if (isStopping)
						return;
				}

				var nextBuffers = new BufferCollection(buffers);
				var newWriteBuffer = nextBuffers.readBuffer[0];

				for (int i = 1; i < readBuffersCount; i++)
				{
					nextBuffers.readBuffer[i - 1] = nextBuffers.readBuffer[i];
				}

				nextBuffers.readBuffer[readBuffersCount - 1] = nextBuffers.writeBuffer;
				nextBuffers.writeBuffer = newWriteBuffer;
				nextBuffers.offset = controller.Sample - readBuffersCount * bufferSize;
				buffers = nextBuffers;

				nextBufferReady.Set();

				for (int n = 0; n < bufferSize; n++)
				{
					buffers.writeBuffer.samples[(controller.Sample - buffers.offset) % bufferSize]
						= source.PlayInCurrentThread(new Context(controller));

					controller.NextSample();
				}
			}
		}

		private void WaitForBuffer(IContext context)
		{
			while (context.Sample >= buffers.offset + bufferSize * readBuffersCount)
			{
				topReadingSample =
					Math.Max(topReadingSample, context.Sample);
				nextBufferReady.Reset();
				nextBufferNeeded.Set();
				nextBufferReady.WaitOne(10);
			}
		}

		public void Stop()
		{
			isStopping = true;
		}
	}
}
