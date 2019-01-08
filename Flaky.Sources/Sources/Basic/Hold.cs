using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal class Hold : Source
	{
		private class State
		{
			public Vector2 Sample { get; set; } = new Vector2(0, 0);
		}

		private State state;

		public Vector2 Sample {
			get { return state.Sample; }
			set { state.Sample = value; }
		}

		public Hold(string id) : base(id) { }

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
		}

		protected override Vector2 NextSample(IContext context)
		{
			return state.Sample;
		}

		public override void Dispose() { }
	}
}
