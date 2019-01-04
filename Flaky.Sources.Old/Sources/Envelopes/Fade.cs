using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class FO : Fade
	{
		public FO(float time, string id) : base(time, id) { }
		protected override float FinalValue { get { return 0; } }
		protected override float InitialValue { get { return 1; } }
	}

	public class FI : Fade
	{
		public FI(float time, string id) : base(time, id) { }
		protected override float FinalValue { get { return 1; } }
		protected override float InitialValue { get { return 0; } }
	}

	public abstract class Fade : Source
	{
		protected State state;
		private float speed;
		private float time;

		protected class State
		{
			public bool Initialized = false;
			public float CurrentValue;
		}

		public Fade(float time, string id) : base(id)
		{
			this.time = time;

			if (this.time < 0)
			{
				this.time = 0;
			}
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);

			if (time > 0)
				speed = 1 / (time * context.SampleRate);

			if (!state.Initialized)
			{
				state.CurrentValue = InitialValue;
				state.Initialized = true;
			}
		}

        public override void Dispose() { }

		protected override Vector2 NextSample(IContext context)
		{
			if (time == 0)
				state.CurrentValue = FinalValue;

			if (InitialValue > FinalValue)
			{
				state.CurrentValue -= speed;

				if (state.CurrentValue < FinalValue)
					state.CurrentValue = FinalValue;
			}
			else
			{
				state.CurrentValue += speed;

				if (state.CurrentValue > FinalValue)
					state.CurrentValue = FinalValue;
			}

			return new Vector2(state.CurrentValue, state.CurrentValue);
		}

		protected abstract float InitialValue { get; }
		protected abstract float FinalValue { get; }
	}
}
