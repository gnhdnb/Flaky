using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class WaveReaderFactory : IWaveReaderFactory
	{
		private readonly string libraryPath;

		private readonly Dictionary<string, IWaveReader> 
			waveReaderCache = new Dictionary<string, IWaveReader>();

		private readonly Dictionary<string, IMultipleWaveReader>
			multipleWaveReaderCache = new Dictionary<string, IMultipleWaveReader>();

		public WaveReaderFactory(string libraryPath)
		{
			this.libraryPath = libraryPath;
		}

		public IWaveReader Create(IContext context, string fileName)
		{
			var fullPath = Path.Combine(GetLocation(), "samples", $"{fileName}.wav");

			if (!waveReaderCache.ContainsKey(fileName))
				return waveReaderCache[fileName] = new WaveReader((IFlakyContext)context, fullPath);

			return waveReaderCache[fileName];
		}

		public IMultipleWaveReader Create(IContext context, string folder, string pack)
		{
			var fullPath = Path.Combine(GetLocation(), folder);

			if (!multipleWaveReaderCache.ContainsKey(pack))
				multipleWaveReaderCache[pack] = new MultipleWaveReader((IFlakyContext)context, fullPath, pack);

			return multipleWaveReaderCache[pack];
		}

		private string GetLocation()
		{
			return libraryPath;
		}
	}
}
