using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Flaky
{
	public class OpenTKAudioDevice : IAudioDevice
	{
		private Thread worker;
		private IBufferedSource source;
		private bool initialized = false;
		private bool running = false;

		public void Dispose()
		{
			Stop();
		}

		public void Init(IBufferedSource source, string recordFilePath = null)
		{
			if (initialized)
				throw new InvalidOperationException("Already initialized.");

			initialized = true;

			worker = new Thread(Run);
			this.source = source;
		}

		public void Play()
		{
			running = true;
			worker.Start();
		}

		public void Stop()
		{
			running = false;

			try
			{
				worker.Join();
			} catch(ThreadStateException) { }
		}

		private void Run()
		{
			const int audioBuffersCount = 3;

			using (new AudioContext())
			{ 
				int s = AL.GenSource();

				int[] buffers = AL.GenBuffers(audioBuffersCount);

				var zeroes = new float[source.BufferSize];

				foreach(var buffer in buffers)
					AL.BufferData(buffer, ALFormat.Stereo16, zeroes, zeroes.Length, source.SampleRate);

				AL.SourceQueueBuffers(s, buffers.Length, buffers);
				AL.SourcePlay(s);
				int processedCount = 0;

				while (running)
				{
					AL.GetSource(s, ALGetSourcei.BuffersProcessed, out processedCount);

					while(processedCount > 0)
					{
						var managedBuffer = ToByteArray(ToInt16(source.ReadNextBatch()));
						int buffer = AL.SourceUnqueueBuffer(s);

						AL.BufferData(buffer, ALFormat.Stereo16, managedBuffer, managedBuffer.Length, source.SampleRate);
						AL.SourceQueueBuffer(s, buffer);
						processedCount--;
					}

					if(AL.GetSourceState(s) == ALSourceState.Stopped)
						AL.SourcePlay(s);

					Thread.Sleep(1);
				}

				AL.SourceStop(s);
				AL.DeleteBuffers(buffers);
				AL.DeleteSource(s);
			}
		}

		private byte[] ToByteArray(Int16[] arr)
		{
			var byteArray = new byte[arr.Length * 2];

			Buffer.BlockCopy(arr, 0, byteArray, 0, byteArray.Length);

			return byteArray;
		}

		private Int16[] ToInt16(float[] arr)
		{
			var result = new Int16[arr.Length];

			for (int i = 0; i < arr.Length; i++)
			{
				result[i] = (Int16)(arr[i] * Int16.MaxValue);
			}

			return result;
		}
	}
}
