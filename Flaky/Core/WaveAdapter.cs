using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class WaveAdapter : IWaveProvider
	{
		private WaveFormat waveFormat;
		private Source source;
		private readonly ContextController controller = new ContextController();

		public WaveAdapter()
		{
			waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);
			controller.Register<IWaveReaderFactory>(new WaveReaderFactory());
			source = 0;
		}

		public void ChangePlayer(IPlayer player)
		{
			var source = player.CreateSource();
			source.Initialize(new Context(controller));
			this.source = source;
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
			for (int n = 0; n < sampleCount; n++)
			{
				var value = source.Play(new Context(controller)).Value;
				buffer[n + offset] = value;
				controller.NextSample();
			}

			return sampleCount;
		}
	}
}
