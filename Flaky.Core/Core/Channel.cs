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
		private bool disposed = false;
		private int codeVersion = 0;
		private readonly float[] nullBuffer;
		private readonly int bufferSize;

		internal Channel(int sampleRate, int bufferSize, int bpm, Configuration configuration)
		{
			controller = new ContextController(sampleRate, bpm, configuration);
			source = null;
			nullBuffer = new float[bufferSize];
			this.bufferSize = bufferSize;
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

		public string[] ChangePlayer(IPlayer player)
		{
			if (sourceToDispose != null)
			{
				sourceToDispose.Dispose();
				sourceToDispose = null;
			}

			ISource source;

			try
			{
				source = player.CreateSource();
			} catch (Exception ex)
			{
				return new[] { ex.ToString() };
			}

			codeVersion++;
			source.Initialize(null, new Context(controller, codeVersion));
			sourceToDispose = this.source;
			this.source = source;

			return new string[0];
		}

		internal float[] ReadNextBatch()
		{
			lock (buffers)
			{
				if (buffers.Count == 0)
					return nullBuffer;

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

				var buffer = new float[bufferSize];

				if (source != null)
				{
					for (int n = 0; n < bufferSize; n += 2)
					{
						var value = source.Play(new Context(controller));
						buffer[n] = value.X;
						buffer[n + 1] = value.Y;
						controller.NextSample();
					}
				}

				lock (buffers)
				{
					buffers.Enqueue(buffer);
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

			if(controller != null)
				controller.Dispose();
		}
	}
}
