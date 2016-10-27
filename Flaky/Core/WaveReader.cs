using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal class WaveReader : IWaveReader
	{
		private readonly float[] sample;

		public WaveReader(string fileName)
		{
			var reader = new WaveFileReader(fileName);

			List<float> sample = new List<float>();
			float[] frame;
			do
			{
				frame = reader.ReadNextSampleFrame();
				if(frame != null)
					sample.Add(frame[0]);
			} while (frame != null);

			this.sample = sample.ToArray();
		}

		public float? Read(long index)
		{
			if (index <= sample.LongLength)
				return sample[index];
			else
				return null;
		}
	}
}
