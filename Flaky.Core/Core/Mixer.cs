using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Mixer : IBufferedSource, IDisposable
	{
		private const float volumeChangeSpeed = 0.000045f;
		private Channel[] channels;
		private volatile float[] expectedVolume;
		private volatile float[] volume;
		private readonly int sampleRate;
        private readonly int bufferSize;

		public Mixer(
			int channelsCount, 
			int sampleRate,
			int bufferSize, 
			int bpm, Configuration configuration)
		{
			if (channelsCount <= 0 || channelsCount > 8)
				throw new ArgumentOutOfRangeException(nameof(channelsCount));

			this.sampleRate = sampleRate;
            this.bufferSize = bufferSize;

			volume = new float[channelsCount];
			expectedVolume = new float[channelsCount];

			channels = Enumerable
				.Range(0, channelsCount)
				.Select(i => new Channel(sampleRate, bufferSize, bpm, configuration))
				.ToArray();
		}

        public int BufferSize
        {
            get
            {
                return bufferSize;
            }
        }

		public int SampleRate
		{
			get
			{
				return sampleRate;
			}
		}

		public float[] ReadNextBatch()
		{
			var buffers = channels
				.Select(c => c.ReadNextBatch())
				.ToList();

			if (buffers.Count == 1)
				return buffers[0];

			var result = new float[buffers[0].Length];

			for (int i = 0; i < buffers[0].Length; i++)
			for (int c = 0; c < buffers.Count; c++)
			{
				ChangeVolume(c);

				result[i] += buffers[c][i] * volume[c];
			}

			return result;
		}

		public void ChangePlayer(int channel, IPlayer player)
		{
			channels[channel].ChangePlayer(player);
		}

		public void SetVolume(int channel, float volume)
		{
			expectedVolume[channel] = volume;
		}

		public void Dispose()
		{
			foreach(var channel in channels)
			{
				channel.Dispose();
			}
		}

		private void ChangeVolume(int channel)
		{
			if(volume[channel] - expectedVolume[channel] < -volumeChangeSpeed)
				volume[channel] += volumeChangeSpeed;

			if (volume[channel] - expectedVolume[channel] > volumeChangeSpeed)
				volume[channel] -= volumeChangeSpeed;
		}
	}
}
