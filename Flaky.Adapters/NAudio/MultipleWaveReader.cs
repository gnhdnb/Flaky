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

		internal MultipleWaveReader(IFlakyContext context, string folder, string pack)
		{
			waves =
				GetAllWavefiles(context, Path.Combine(folder, pack))
				.Select(f => new WaveReader(context, f))
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

		private List<string> GetAllWavefiles(IFlakyContext context, string folder)
		{
			if(!Directory.Exists(folder))
			{
				context.ShowError($"{folder} does not exist");

				return new List<string>();
			}

			return Directory
				.GetDirectories(folder)
				.SelectMany(d => GetAllWavefiles(context, d))
				.Concat(
					Directory.GetFiles(folder)
					.Where(f => f.EndsWith(".wav")))
				.OrderBy(f => f)
				.ToList();
		}
	}
}
