using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Flaky
{
	public class VariationalWaveTable : Source, IPipingSource<Source>
	{
		private readonly Source selector;
		private readonly string pack;
		private Source pitch;
		private State state;

		internal VariationalWaveTable(string pack, Source selector, string id) : base(id)
		{
			this.pack = pack;
			this.selector = selector;
		}

		private class State
		{
			private IMultipleWaveReader waveReader;
			private string pack;

			private double currentWaveformPosition;
			private double currentCrossfadeStart;
			private int currentWaveformLength;
			private int currentWaveformIndex = -1;
			private double nextWaveformPosition;
			private int nextWaveformIndex;

			private int[] crossingPointsCache;

			public void Init(IContext context, string pack, IWaveReaderFactory readerFactory)
			{
				if (this.pack == pack)
					return;

				this.pack = pack;

				var newReader = readerFactory.Create(context, "variationalwaveforms", pack);
				crossingPointsCache = EvaulateCrossingPointsCache(newReader);
				waveReader = newReader;

				currentWaveformPosition = 0;
				currentCrossfadeStart = 0;
				currentWaveformLength = 0;
				currentWaveformIndex = -1;
				nextWaveformPosition = 0;
				nextWaveformIndex = 0;
			}

			public Vector2 Read(float pitch, float selector)
			{
				if (waveReader == null || waveReader.Waves == 0)
					return Vector2.Zero;

				if (pitch < 0)
					pitch = -pitch;

				if(currentWaveformIndex == -1)
				{
					SwitchWaveform(selector);
					SwitchWaveform(selector);
				}

				currentWaveformPosition += pitch / 73.5f;
				nextWaveformPosition += pitch / 73.5f;

				selector %= 1;

				if (selector < 0)
					selector += 1;

				if (currentWaveformPosition >= currentWaveformLength)
					SwitchWaveform(selector);

				var currentSample = Read(waveReader, currentWaveformIndex, currentWaveformPosition);

				if (nextWaveformPosition < 0)
					return currentSample;

				var nextSample = Read(waveReader, nextWaveformIndex, nextWaveformPosition);

				var crossfade = (float)
					((currentWaveformPosition - currentCrossfadeStart) / (currentWaveformLength - currentCrossfadeStart));

				return currentSample * (1 - crossfade) + nextSample * crossfade;
			}

			private Vector2 Read(IMultipleWaveReader reader, int wave, double position)
			{
				position %= reader.Length(wave);

				var index1 = (long)Math.Floor(position);
				var index2 = (long)Math.Ceiling(position);

				if (index2 >= reader.Length(wave))
					index2 -= reader.Length(wave);

				var crossfade = (float)(position % 1);

				var sample1 = reader.Read(wave % reader.Waves, index1).Value;
				var sample2 = reader.Read(wave % reader.Waves, index2).Value;

				return sample2 * crossfade + sample1 * (1 - crossfade);
			}

			private void SwitchWaveform(float selector)
			{
				currentWaveformIndex = nextWaveformIndex;

				if (nextWaveformPosition >= 0)
					currentWaveformPosition = nextWaveformPosition;
				else
					currentWaveformPosition = 0;

				currentCrossfadeStart = crossingPointsCache[currentWaveformIndex];

				if (currentCrossfadeStart < currentWaveformPosition)
					currentCrossfadeStart = currentWaveformPosition;

				currentWaveformLength = (int)waveReader.Length(currentWaveformIndex);

				nextWaveformIndex = (int)Math.Floor((waveReader.Waves - 1) * selector);
				nextWaveformPosition = -(currentCrossfadeStart - currentWaveformPosition);
			}

			private int[] EvaulateCrossingPointsCache(IMultipleWaveReader reader)
			{
				var result = new int[reader.Waves];

				for(int i = 0; i < reader.Waves; i++)
				{
					result[i] = GetLastCrossingPoint(reader.Wave(i));
				}

				return result;
			}

			private int GetLastCrossingPoint(Vector2[] wave)
			{
				int lastCrossingPoint = wave.Length - 1;

				for (int i = wave.Length - 2; i > 0; i--)
				{
					if (Math.Sign(wave[i].X) != Math.Sign(wave[i + 1].X))
					{
						lastCrossingPoint = i + 1;
						break;
					}
				}

				return lastCrossingPoint;
			}
		}

		protected override Vector2 NextSample(IContext context)
		{
			var freq = pitch.Play(context).X;

			return state.Read(
				freq,
				selector.Play(context).X);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);

			state.Init(context, pack, Get<IWaveReaderFactory>(context));

			Initialize(context, pitch, selector);
		}

		public override void Dispose()
		{
			Dispose(pitch, selector);
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			pitch = mainSource;
		}
	}
}
