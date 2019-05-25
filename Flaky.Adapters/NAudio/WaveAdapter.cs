using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flaky
{
	public class WaveAdapter : IWaveProvider
	{
		private WaveFormat waveFormat;
		private readonly IBufferedSource source;
		private float[] internalBuffer;
		private int internalBufferIndex = 0;

		public WaveAdapter(IBufferedSource source)
		{
			this.source = source;
			waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(source.SampleRate, 2);
		}

		public WaveFormat WaveFormat
		{
			get
			{
				return waveFormat;
			}
		}

		public int Read(byte[] buffer, int offset, int count)
		{
			WaveBuffer waveBuffer = new WaveBuffer(buffer);
			int samplesRequired = count / 4;
			int samplesRead = Read(waveBuffer.FloatBuffer, offset / 4, samplesRequired);
			return samplesRead * 4;
		}

		public int Read(float[] buffer, int offset, int sampleCount)
		{
			for (int i = 0; i < sampleCount; i++)
			{
				if (internalBuffer == null || internalBufferIndex >= internalBuffer.Length)
				{
					internalBuffer = source.ReadNextBatch();

					internalBufferIndex = 0;
				}

				if (internalBuffer != null)
					buffer[i + offset] = internalBuffer[internalBufferIndex];

				internalBufferIndex++;
			}

			return sampleCount;
		}
	}
}
