using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Mixer : IBufferedSource, IDisposable
	{
		private Channel[] channels;
		private const int sampleRate = 44100;

		public Mixer(int channelsCount, Configuration configuration)
		{
			if (channelsCount <= 0 || channelsCount > 8)
				throw new ArgumentOutOfRangeException(nameof(channelsCount));

			channels = Enumerable
				.Range(0, channelsCount)
				.Select(i => new Channel(sampleRate, configuration))
				.ToArray();
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

			for(int i = 0; i < buffers[0].Length; i++)
			for(int c = 1; c < buffers.Count; c++)
			{
				buffers[0][i] += buffers[c][i];
			}

			return buffers[0];
		}

		public void ChangePlayer(int channel, IPlayer player)
		{
			channels[channel].ChangePlayer(player);
		}

		public void Dispose()
		{
			foreach(var channel in channels)
			{
				channel.Dispose();
			}
		}
	}
}
