using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flaky
{
	internal class Channel : IDisposable
	{
		private ISource source;
        private ISource sourceToDispose;
        private readonly ContextController controller;
		private readonly Thread worker;
		private readonly Queue<float[]> buffers = new Queue<float[]>();
		private readonly Semaphore buffersCounter = new Semaphore(0, 3);
		private readonly Semaphore reverseBuffersCounter = new Semaphore(0, 3);
		private bool disposed = false;
		private int codeVersion = 0;

		internal Channel(int sampleRate, Configuration configuration)
		{
			controller = new ContextController(sampleRate, 120, configuration);
			source = null;
			worker = new Thread(Play);
			worker.Priority = ThreadPriority.Highest;
			worker.Start();
		}

		internal int SampleRate
		{
			get
			{
				return controller.SampleRate;
			}
		}

		public void ChangePlayer(IPlayer player)
		{
            if (sourceToDispose != null)
            {
                sourceToDispose.Dispose();
                sourceToDispose = null;
            }

            var source = player.CreateSource();
			codeVersion++;
			source.Initialize(new Context(controller, codeVersion));
            sourceToDispose = this.source;
            this.source = source;
		}

		internal float[] ReadNextBatch()
		{
			reverseBuffersCounter.WaitOne();

			lock (buffers)
			{
				var result = buffers.Dequeue();
				buffersCounter.Release();
				return result;
			}
		}

		private void Play()
		{
			while (!disposed)
			{
				buffersCounter.WaitOne(100);

				lock (buffers)
				{
					if (buffers.Count > 2)
						continue;
				}

				var buffer = new float[13230];

				if (source != null)
				{
					for (int n = 0; n < 13230; n += 2)
					{
						var value = source.Play(new Context(controller));
						buffer[n] = value.Left;
						buffer[n + 1] = value.Right;
						controller.NextSample();
					}
				}

				lock (buffers)
				{
					buffers.Enqueue(buffer);
					reverseBuffersCounter.Release();
				}
			}
		}

		public void Dispose()
		{
			if (disposed)
				return;
			disposed = true;

			worker.Join();

            if (sourceToDispose != null)
                sourceToDispose.Dispose();

            if (source != null)
                source.Dispose();
        }
	}
}
