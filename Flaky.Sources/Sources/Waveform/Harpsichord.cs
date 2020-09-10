using System;
using System.IO;
using System.Numerics;

namespace Flaky
{
	public class HarpsichordSource : Source
	{
		private readonly Source clip;
		private readonly Source width;
		private readonly Source bass;
		private readonly Source crossFade;
		private readonly Source delta;
		private readonly ChromaticFieldSource field;
		private readonly string pack;

		private class State
		{
			public Harpsichord harp;
			public float[] bandAmp;
			private bool initialized;

			public void Initialize(IContext context, string pack)
			{
				if (initialized)
					return;

				harp = new Harpsichord(pack, context);
				bandAmp = new float[60];

				initialized = true;
			}
		}

		private State state;

		public HarpsichordSource(string pack,
			ChromaticFieldSource field,
			Source clip,
			Source width,
			Source bass,
			Source crossFade,
			Source delta,
			string id) : base(id)
		{
			this.pack = pack;
			this.clip = clip;
			this.bass = bass;
			this.delta = delta;
			this.crossFade = crossFade;
			this.width = width;
			this.field = field;
		}

		public override void Dispose()
		{
			Dispose(clip, width, bass, field, crossFade, delta);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			state.Initialize(context, pack);
			Initialize(context, clip, width, bass, field, crossFade, delta);
		}

		protected override Vector2 NextSample(IContext context)
		{
			var result = Vector2.Zero;

			var ampArray = field.GetField(context);

			var cf = crossFade.Play(context).X;

			var clipValue = clip.Play(context).X;

			var widthValue = width.Play(context).X;

			var bassValue = bass.Play(context).X;

			var deltaValue = delta.Play(context).X;

			for (int band = 0; band < 59; band++)
			{
				var amp = ampArray[band];

				if (amp < widthValue)
					amp = amp * amp * amp;

				if (amp > 1.5f)
					amp = 1.5f;

				if (amp > clipValue * 2)
					amp = 0;

				if (amp < 0)
					amp = 0;

				if (band > 40)
					amp = amp * bassValue * 2;

				state.bandAmp[band] = state.bandAmp[band] +
					(amp - state.bandAmp[band]) * ((1 - cf) + cf/(50 * 3500 + 100 * (band % 10)));
				//state.bandAmp[band] = amp;

				if (band > 1)
					result += state.harp.Read(60 - band - 1, deltaValue) * state.bandAmp[band];
			}

			return result;
		}
	}

	public class Harpsichord
	{
		private readonly IWaveReader[] readers;
		private readonly float[] counters;

		public Harpsichord(string path, IContext context)
		{
			readers = new IWaveReader[59];
			counters = new float[59];

			for (int i = 0; i <= 58; i++)
			{
				readers[i] = ((IFlakyContext)context).Get<IWaveReaderFactory>()
					.Create(context, Path.Combine(path, $"{i}"));
			}
		}

		public Vector2 Read(int band, float delta)
		{
			counters[band] += delta;

			if (counters[band] >= readers[band].Length)
				counters[band] = 0;

			var cf = (float)(Math.Abs(2 * counters[band] - readers[band].Length) / (double)readers[band].Length);

			var a = readers[band].Read(counters[band]).Value;
			var b = readers[band].Read((counters[band] + readers[band].Length / 2) % readers[band].Length).Value;

			return a * (1 - cf) + b * cf;
		}
	}
}
