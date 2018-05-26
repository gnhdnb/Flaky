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
		private ISource source;
		private ContextController controller;
		private const int readBuffersCount = 3;
		private const int bufferSize = 6615;

		private BufferCollection buffers;
		private ManualResetEvent nextBufferNeeded = new ManualResetEvent(true);
		private ManualResetEvent nextBufferReady = new ManualResetEvent(true);

		private struct Buffer
		{
			public Sample[] samples;
		}

		private class BufferCollection
		{
			public long offset;
			public Buffer[] buffers = new Buffer[readBuffersCount + 1]; // last one is the write buffer

			public BufferCollection(BufferCollection other)
			{
				this.offset = other.offset;
				this.buffers = other.buffers.ToArray();
			}
		}

		private Thread worker;

		public SeparateThreadProcessor(
			ISource source, 
			ContextController contextController)
		{
			this.source = source;
			this.controller = contextController.Clone();
		}

		public Sample Play(IContext context)
		{
			WaitForBuffer(context);

			return buffers.buffers[(context.Sample - buffers.offset) / bufferSize]
				.samples[(context.Sample - buffers.offset) % bufferSize];
		}

		private void Run()
		{
			nextBufferNeeded.Reset();
			nextBufferReady.Reset();

			var nextBuffers = new BufferCollection(buffers);

			var newWriteBuffer = nextBuffers.buffers[0];

			for (int i = 1; i < nextBuffers.buffers.Length; i++)
			{
				nextBuffers.buffers[i - 1] = nextBuffers.buffers[i];
			}

			nextBuffers.buffers[readBuffersCount] = newWriteBuffer;

			nextBuffers.offset = controller.Sample - readBuffersCount * bufferSize;

			buffers = nextBuffers;

			nextBufferReady.Set();

			for (int n = 0; n < bufferSize; n++)
			{
				buffers.buffers[(controller.Sample - buffers.offset) / bufferSize]
					.samples[(controller.Sample - buffers.offset) % bufferSize] = source.Play(new Context(controller));

				controller.NextSample();
			}

			nextBufferNeeded.WaitOne();
		}

		private void WaitForBuffer(IContext context)
		{
			if (context.Sample >= buffers.offset + bufferSize * readBuffersCount)
				nextBufferNeeded.Set();

			while (context.Sample >= buffers.offset + bufferSize * readBuffersCount)
			{
				nextBufferReady.WaitOne(100);
			}
		}
	}
}
