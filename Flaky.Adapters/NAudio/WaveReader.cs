﻿using NAudio.Wave;
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
	public class WaveReader : IWaveReader
	{
		private readonly Vector2[] sample;

		public WaveReader(string fullPath)
		{
			var reader = new WaveFileReader(fullPath);

			List<Vector2> sample = new List<Vector2>();
			float[] frame;
			do
			{ 
				frame = reader.ReadNextSampleFrame();
				if (frame != null)
				{
					if(frame.Length >1)
						sample.Add(new Vector2 {
							X = frame[0],
							Y = frame[1]
						});
					else
						sample.Add(new Vector2
						{
							X = frame[0],
							Y = frame[0]
						});
				}
			} while (frame != null);

			this.sample = sample.ToArray();
		}

		public Vector2? Read(long index)
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
