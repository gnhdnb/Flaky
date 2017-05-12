using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal class WaveReader : IWaveReader
	{
		private readonly float[] sample;

		public WaveReader(string fileName)
		{
			var fullPath = Path.Combine(GetLocation(), $@"samples\{fileName}.wav");
			var reader = new WaveFileReader(fullPath);

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

		private static string GetLocation()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}
	}
}
