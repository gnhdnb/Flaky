using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class WaveTable : Source, IPipingSource
	{
		private readonly Source selector;
		private readonly string pack;
		private Source pitch;
		private State state;

		private class State
		{
			private IMultipleWaveReader waveReader;
			private string pack;
			private double position;

			public void Init(string pack, IWaveReaderFactory readerFactory)
			{
				if (this.pack == pack)
					return;

				this.pack = pack;

				waveReader = readerFactory.Create("waveforms", pack);
			}

			public Vector2 Read(float pitch, float selector)
			{
				if (pitch < 0)
					pitch = -pitch;

				position += pitch / 73.5f;

				selector %= 1;

				if (selector < 0)
					selector += 1;

				var index = waveReader.Waves * selector;

				var index1 = (int)Math.Floor(index);
				var index2 = (int)Math.Ceiling(index);

				if (index2 >= waveReader.Waves)
					index2 -= waveReader.Waves;

				var crossfade = index % 1;

				var maxLength = Math.Max(
						waveReader.Length(index1),
						waveReader.Length(index2));

				if (position > maxLength)
					position %= maxLength;

				var sample1 = Read(waveReader, index1, position);
				var sample2 = Read(waveReader, index2, position);

				return sample2 * crossfade + sample1 * (1 - crossfade);
			}

			private Vector2 Read(IMultipleWaveReader reader, int wave, double position)
			{
				position %= reader.Length(wave);

				var index1 = (long)Math.Floor(position);
				var index2 = (long)Math.Ceiling(position);

				if (index2 >= reader.Length(wave))
					index2 -= reader.Length(wave);

				var crossfade = (float)(position % 1);

				var sample1 = reader.Read(wave, index1).Value;
				var sample2 = reader.Read(wave, index2).Value;

				return sample2 * crossfade + sample1 * (1 - crossfade);
			}
		}

		public WaveTable(string pack, Source selector, string id) : base(id)
		{
			this.pack = pack;
			this.selector = selector;
		}

		public override void Dispose()
		{
			Dispose(selector, pitch);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			state.Init(pack, Get<IWaveReaderFactory>(context));

			Initialize(context, pitch, selector);
		}

		protected override Vector2 NextSample(IContext context)
		{
			return state.Read(
				pitch.Play(context).X,
				selector.Play(context).X);
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			pitch = mainSource;
		}
	}
}
