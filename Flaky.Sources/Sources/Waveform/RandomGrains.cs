using Redzen.Random.Double;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Flaky
{
	public class RandomGrains : Source, IPipingSource<NoteSource>
	{
		private NoteSource source;
		private Source modulation;
		private Source probability;
		private State state;
		private float pitch;
		private string pack;

		internal RandomGrains(string pack, Source modulation, Source probability, float pitch, string id) : base(id)
		{
			if (pitch < 0)
				pitch = 0;

			if (pitch > 2)
				pitch = 2;

			this.pack = pack;
			this.modulation = modulation;
			this.probability = probability;
			this.pitch = pitch;
		}
		
		private class PlayingGrain
		{
			public int Index {get; set;}
			public double Position {get; set;}
		}

		private class State
		{
			private IMultipleWaveReader waveReader;
			private string pack;
			private List<PlayingGrain> currentGrains = new List<PlayingGrain>();

			private readonly ZigguratGaussianDistribution distribution =
				new ZigguratGaussianDistribution(0, 1);

			public void Init(IContext context, string pack, IWaveReaderFactory readerFactory)
			{
				if (this.pack == pack)
					return;

				this.pack = pack;

				waveReader = readerFactory.Create(context, "grains", pack);
			}

			internal Vector2 Play(IContext context, float modValue, float probValue, float pitch)
			{
				if (waveReader.Waves == 0)
					return Vector2.Zero;

				if (modValue < 0)
					modValue = 0;

				if (modValue > 1)
					modValue = 1;

				var output = Vector2.Zero;

				for (int i = currentGrains.Count - 1; i >= 0; i--) {
					var grain = currentGrains[i];
					var currentGrainLength = waveReader.Length(grain.Index);
					output += Interpolate(grain.Index, currentGrainLength * grain.Position);
					grain.Position += pitch / (double)currentGrainLength;

					if(grain.Position >= 1)
						currentGrains.RemoveAt(i);
				}

				if (context.Sample % 44 == 0)
				{
					if (probValue > 1)
						probValue = 1;

					if (probValue < 0.0001f)
						probValue = 0.0001f;

					if (distribution.Sample() > 1 / probValue - 1)
					{
						currentGrains.Add(new PlayingGrain {
							Index = GetGrainIndex(modValue),
							Position = 0
						});
					}
				}

				return output;
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

			private int GetGrainIndex(float modValue)
			{
				var random = distribution.Sample() * 25;

				int index = (int)Math.Round(waveReader.Waves * modValue + random);

				if (index < 0)
					index = 0;

				if (index >= waveReader.Waves)
					index = waveReader.Waves - 1;

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
			state.Init(context, pack, Get<IWaveReaderFactory>(context));

			Initialize(context, source, modulation, probability);
		}

		protected override Vector2 NextSample(IContext context)
		{
			var playingNote = source.GetNote(context);
			var modValue = modulation.Play(context).X;
			var probValue = probability.Play(context).X;

			return state.Play(context, modValue, probValue, this.pitch * playingNote.Note.ToPitch());
			//return state.Play(context, modValue, 1);
		}

		public override void Dispose()
		{
			Dispose(source, modulation, probability);
		}
	}
}
