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
		private readonly Mixer mixer;

		internal WaveAdapter(Mixer mixer)
		{
			this.mixer = mixer;
			waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(mixer.SampleRate, 2);
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
			float[] internalBuffer = mixer.ReadNextBatch();

			if (sampleCount != internalBuffer.Length)
				throw new InvalidOperationException("Buffer size mismatch.");

			if (internalBuffer != null)
			{
				for(int i = 0; i< internalBuffer.Length; i++)
				{
					buffer[i + offset] = internalBuffer[i];
				}
			}

			return internalBuffer.Length;
		}
	}
}
