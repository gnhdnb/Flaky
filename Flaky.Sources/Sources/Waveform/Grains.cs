using Redzen.Random.Double;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Flaky
{
	public class Grains : Source, IPipingSource<NoteSource>
	{
		private NoteSource source;
		private Source modulation;
		private State state;
		private float pitch;
		private string pack;

		internal Grains(string pack, Source modulation, float pitch, string id) : base(id)
		{
			if (pitch < 0)
				pitch = 0;

			if (pitch > 2)
				pitch = 2;

			this.pack = pack;
			this.modulation = modulation;
			this.pitch = pitch;
		}
		
		private class State
		{
			private IMultipleWaveReader waveReader;
			private string pack;
			private double position;
			private int currentGrain = -1;
			private int nextGrain = -1;

			private readonly ZigguratGaussianDistribution distribution =
				new ZigguratGaussianDistribution(0, 1);

			public void Init(string pack, IWaveReaderFactory readerFactory)
			{
				if (this.pack == pack)
					return;

				this.pack = pack;

				waveReader = readerFactory.Create("grains", pack);
			}

			internal Vector2 Play(IContext context, float modValue, float pitch)
			{
				if (modValue < 0)
					modValue = 0;

				if (modValue > 1)
					modValue = 1;

				if (currentGrain == -1)
				{
					currentGrain = GetGrainIndex(modValue, currentGrain);
					nextGrain = GetGrainIndex(modValue, nextGrain);
				} 

				if (position >= 0.5)
				{
					currentGrain = nextGrain;
					nextGrain = GetGrainIndex(modValue, nextGrain);
					position = 0;
				}

				var currentGrainLength = waveReader.Length(currentGrain);
				var nextGrainLength = waveReader.Length(nextGrain);

				var current = Interpolate(currentGrain, currentGrainLength * (position + 0.5));
				var next = Interpolate(nextGrain, nextGrainLength * position);

				position += pitch / (double)currentGrainLength;

				return current + next;
			}

			private Vector2 Interpolate(int grain, double position)
			{
				var index1 = (long)Math.Floor(position);
				var index2 = (long)Math.Ceiling(position);

				if (index2 >= waveReader.Length(grain))
					index2 = waveReader.Length(grain) - 1;

				var crossfade = (float)(position % 1);

				var sample1 = waveReader.Read(grain, index1).Value;
				var sample2 = waveReader.Read(grain, index2).Value;

				return sample2 * crossfade + sample1 * (1 - crossfade);
			}

			private int GetGrainIndex(float modValue, int currentIndex)
			{
				int index = 0;

				do
				{
					var random = distribution.Sample() * 3;

					index = (int)Math.Round(waveReader.Waves * modValue + random);

					if (index < 0)
						index = 0;

					if (index >= waveReader.Waves)
						index = waveReader.Waves - 1;
				} while (index == currentIndex);

				return index;
			}
		}

		public void SetMainSource(NoteSource mainSource)
		{
			this.source = mainSource;
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			state.Init(pack, Get<IWaveReaderFactory>(context));

			Initialize(context, source, modulation);
		}

		protected override Vector2 NextSample(IContext context)
		{
			var playingNote = source.GetNote(context);
			var modValue = modulation.Play(context).X;

			return state.Play(context, modValue, this.pitch * playingNote.Note?.ToPitch() ?? 0);
			//return state.Play(context, modValue, 1);
		}

		public override void Dispose()
		{
			Dispose(source, modulation);
		}
	}
}
