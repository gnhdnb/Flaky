using NAudio.Wave;
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

		internal WaveReader(IFlakyContext context, string fullPath)
		{
			WaveFileReader reader;

			try
			{
				reader = new WaveFileReader(fullPath);
			}
			catch(Exception ex)
			{
				context.ShowError(ex.ToString());
				this.sample = new Vector2[1024];
				return;
			}

			List<Vector2> sample = new List<Vector2>();
			float[] frame;
			do
			{ 
				frame = reader.ReadNextSampleFrame();
				if (frame != null)
				{
					if(frame.Length > 1)
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

		internal Vector2[] Sample { get { return sample; } }

		public long Length
		{
			get
			{
				return sample.LongLength;
			}
		}
	}
}
