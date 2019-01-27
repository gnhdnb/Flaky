using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Flaky
{
	public class External : Source
	{
		public External(string id) : base(id) { }

		public float Value { get; set; }

		private State state;

		internal class State
		{
			public float Value = 0;
		}

		protected override Vector2 NextSample(IContext context)
		{
			var delta = Value - state.Value;

			state.Value += delta / 22050;

			return new Vector2(state.Value, state.Value);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			state.Value = Value;
		}

		public override void Dispose()
		{
			// nothing to dispose
		}
	}
}
