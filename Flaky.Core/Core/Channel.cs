﻿using System;
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
		private readonly ContextController controller;
		private readonly Thread worker;
		private readonly Queue<float[]> buffers = new Queue<float[]>();
		private readonly Semaphore buffersCounter = new Semaphore(0, 3);
		private bool disposed = false;
		private volatile float volume = 1;

		internal Channel(int sampleRate, Configuration configuration)
		{
			controller = new ContextController(sampleRate, configuration);
			source = null;
			worker = new Thread(Play);
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
			var source = player.CreateSource();
			source.Initialize(new Context(controller));
			this.source = source;
		}

		public void SetVolume(float volume)
		{
			this.volume = volume;
		}

		internal float[] ReadNextBatch()
		{
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
						buffer[n] = value.Left * volume;
						buffer[n + 1] = value.Right * volume;
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
		}
	}
}