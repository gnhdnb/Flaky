using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Flaky
{
	public class Glide : Source, IPipingSource
	{
		private State state;
		private Source velocity;
		private Source mainSource;

		internal Glide(Source velocity, string id) : base(id)
		{
			this.velocity = velocity;
		}

		internal class State
		{
			public Vector2 Value;
		}

		protected override Vector2 NextSample(IContext context)
		{
			var velocityValue = velocity.Play(context).X;
			var mainValue = mainSource.Play(context);

			if (velocityValue < 0)
				velocityValue = 0;

			var delta = mainValue - state.Value;

			state.Value += delta * velocityValue / context.SampleRate;

			return state.Value;
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			Initialize(context, mainSource, velocity);
		}

		public override void Dispose()
		{
			Dispose(mainSource, velocity);
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.mainSource = mainSource;
		}
	}
}
