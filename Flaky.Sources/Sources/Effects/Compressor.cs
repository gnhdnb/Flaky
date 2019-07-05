using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Flaky
{
	public class Compressor : Source, IPipingSource
	{
		private Source mainSource;
		private Source threshold;
		private Source attack;
		private Source release;
		private Source sidechain;
		private State state;

		private class State
		{
			public float detector = 0;
			public float attenuation = 1;
		}

		public Compressor(Source threshold, Source attack, Source release, Source sidechain, string id) : base(id)
		{
			this.threshold = threshold;
			this.attack = attack;
			this.release = release;
			this.sidechain = sidechain;
		}

		protected override Vector2 NextSample(IContext context)
		{
			var signal = mainSource.Play(context);
			var attackValue = attack.Play(context).X;
			var releaseValue = release.Play(context).X;

			if (attackValue < 0.001f)
				attackValue = 0.001f;

			if (releaseValue < 0.001f)
				releaseValue = 0.001f;

			float detectorFeed;

			if (sidechain != null)
				detectorFeed = sidechain.Play(context).Length();
			else
				detectorFeed = signal.Length();

			var thresholdValue = threshold.Play(context).X;

			state.detector = state.detector * 0.99f;

			if (detectorFeed > state.detector)
				state.detector = detectorFeed;

			var compDelta = state.detector - thresholdValue;

			if (compDelta > 0)
			{
				// attack

				var desiredAttn = state.detector / thresholdValue;

				float c = 0.00005f / attackValue;

				state.attenuation += (desiredAttn - state.attenuation) * c;
			}
			else
			{
				// release

				float c = 0.00005f / releaseValue;

				state.attenuation += (1 - state.attenuation) * c;
			}

			return signal * (1 / state.attenuation);
		}

		public override void Dispose()
		{
			Dispose(mainSource, threshold, attack, release, sidechain);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);

			Initialize(context, mainSource, threshold, attack, release, sidechain);
		}

		public void SetMainSource(Source mainSource)
		{
			this.mainSource = mainSource;
		}
	}
}
