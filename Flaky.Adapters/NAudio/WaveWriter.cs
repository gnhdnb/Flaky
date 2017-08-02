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
	internal class WaveWriter : IWaveWriter
	{
		private readonly float[] sample;
		private readonly WaveFileWriter writer;

		public WaveWriter(string fileName, int sampleRate)
		{
			var fullPath = Path.Combine(GetLocation(), $@"{fileName}.wav");
			var writer = new WaveFileWriter(File.OpenWrite(fullPath), WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2));
		}

		private static string GetLocation()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		public void Dispose()
		{
			writer.Dispose();
		}

		public void Write(Sample sample)
		{
			writer.WriteSample(sample.Left);
			writer.WriteSample(sample.Right);
		}
	}
}
