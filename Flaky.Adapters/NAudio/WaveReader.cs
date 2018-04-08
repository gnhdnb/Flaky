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
		private readonly Sample[] sample;

		public WaveReader(string fullPath)
		{
			var reader = new WaveFileReader(fullPath);

			List<Sample> sample = new List<Sample>();
			float[] frame;
			do
			{ 
				frame = reader.ReadNextSampleFrame();
				if (frame != null)
				{
					if(frame.Length >1)
						sample.Add(new Sample {
							Left = frame[0],
							Right = frame[1]
						});
					else
						sample.Add(new Sample
						{
							Left = frame[0],
							Right = frame[0]
						});
				}
			} while (frame != null);

			this.sample = sample.ToArray();
		}

		public Sample? Read(long index)
		{
			if (index < sample.LongLength)
				return sample[index];
			else
				return null;
		}

		public long Length
		{
			get
			{
				return sample.LongLength;
			}
		}
	}
}
