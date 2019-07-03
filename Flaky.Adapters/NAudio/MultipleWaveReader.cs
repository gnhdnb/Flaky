using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal class MultipleWaveReader : IMultipleWaveReader
	{
		private readonly List<WaveReader> waves;

		public MultipleWaveReader(string folder, string pack)
		{
			waves =
				GetAllWavefiles(Path.Combine(folder, pack))
				.Select(f => new WaveReader(f))
				.ToList(); 
		}

		public int Waves => waves.Count;

		public long Length(int wave)
		{
			return waves[wave].Length;
		}

		public Vector2? Read(int wave, long index)
		{
			return waves[wave].Read(index);
		}

		public Vector2[] Wave(int index)
		{
			return waves[index].Sample;
		}

		private List<string> GetAllWavefiles(string folder)
		{
			return Directory
				.GetDirectories(folder)
				.SelectMany(d => GetAllWavefiles(d))
				.Concat(
					Directory.GetFiles(folder)
					.Where(f => f.EndsWith(".wav")))
                .OrderBy(f => f)
				.ToList();
		}
	}
}
