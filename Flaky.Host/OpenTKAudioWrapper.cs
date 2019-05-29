using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Flaky
{
	public class OpenTKAudioWrapper
	{
		private const int audioBuffersCount = 3;

		public void Do()
		{
			var devices = AudioContext.AvailableDevices;

			using (new AudioContext(devices[0]))
			{
				int source = AL.GenSource();
				int[] buffers = AL.GenBuffers(audioBuffersCount);

				var managedBuffer = ToByteArray(ToInt16(GenerateStream()));

				AL.BufferData(buffers[0], ALFormat.Stereo16, managedBuffer, managedBuffer.Length, 44100);

				AL.Source(source, ALSourcei.Buffer, buffers[0]);
				AL.SourcePlay(source);

				Thread.Sleep(1000);

				AL.SourceStop(source);
				AL.DeleteSource(source);
				AL.DeleteBuffers(buffers);
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

			for(int i = 0; i < arr.Length; i++)
			{
				result[i] = (Int16)(arr[i] * Int16.MaxValue);
			}

			return result;
		}

		private float[] GenerateStream()
		{
			var result = new float[44100];

			for (int i = 0; i < result.Length; i++)
			{
				result[i] = (float)Math.Sin(i / 44f);
			}

			return result;
		}
	}
}
