using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal struct Context : IFlakyContext
	{
		private readonly ContextController controller;

		public long Sample { get; }

		public int Beat { get; }
		public int BPM { get; }

		public int SampleRate { get; private set; }

		public TState GetOrCreateState<TState>(string id) where TState : class, new()
		{
			return controller.GetOrCreateState<TState>(id);
		}

		public TFactory Get<TFactory>() where TFactory : class
		{
			return controller.Get<TFactory>();
		}

		internal Context(ContextController controller)
		{
			Sample = controller.Sample;
			Beat = controller.Beat;
			BPM = controller.BPM;
			SampleRate = controller.SampleRate;
			this.controller = controller;
		}
	}
}
