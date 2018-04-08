using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal class WaveReaderFactory : IWaveReaderFactory
	{
		public IWaveReader Create(string fileName)
		{
			var fullPath = Path.Combine(GetLocation(), $@"samples\{fileName}.wav");

			return new WaveReader(fullPath);
		}

		public IMultipleWaveReader Create(string folder, string pack)
		{
			var fullPath = Path.Combine(GetLocation(), folder);

			return new MultipleWaveReader(fullPath, pack);
		}

		private static string GetLocation()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}
	}
}
