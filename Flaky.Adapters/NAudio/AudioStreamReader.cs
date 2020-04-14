using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NVorbis;

namespace Flaky
{
	public class AudioStreamReader : IAudioStreamReader
	{
		public float[] ReadVorbis(Stream stream)
		{
			using (var reader = new NVorbis.VorbisReader(stream))
			{
				var result = new float[reader.TotalSamples];

				reader.ReadSamples(result, 0, (int)reader.TotalSamples);

				return result;
			}
		}

		public float[] ReadWav(Stream stream)
		{
			using (var reader = new NAudio.Wave.WaveFileReader(stream))
			{
				List<float> sample = new List<float>();
				float[] frame;
				do
				{
					frame = reader.ReadNextSampleFrame();

					if(frame != null)
						sample.Add(frame[0]);
				} while (frame != null);

				return sample.ToArray();
			}
		}
	}
}
