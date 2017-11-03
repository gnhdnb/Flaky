using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal class Hold : Source
	{
		private class State
		{
			public Sample Sample { get; set; }
		}

		private State state;

		public Sample Sample {
			get { return state.Sample; }
			set { state.Sample = value; }
		}

		public Hold(string id) : base(id) { }

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
		}

		protected override Sample NextSample(IContext context)
		{
			return state.Sample;
		}

		public override void Dispose() { }
	}
}
