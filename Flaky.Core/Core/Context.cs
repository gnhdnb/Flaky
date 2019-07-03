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
		private readonly int codeVersion;

		public long Sample { get; }

		public int Beat { get; }
		public bool MetronomeTick { get; }

		public int BPM { get; }

		public int SampleRate { get; private set; }

		public TState GetOrCreateState<TState>(string id) where TState : class, new()
		{
			return controller.GetOrCreateState<TState>(id, codeVersion);
		}

		public TFactory Get<TFactory>() where TFactory : class
		{
			return controller.Get<TFactory>();
		}

		public void ShowError(string error)
		{
			controller.ShowError(error);
		}

		internal Context(ContextController controller)
		{
			Sample = controller.Sample;
			Beat = controller.Beat;
			BPM = controller.BPM;
			SampleRate = controller.SampleRate;
			MetronomeTick = controller.MetronomeTick;
			codeVersion = -1;
			this.controller = controller;
		}

		internal Context(ContextController controller, int codeVersion) : this(controller)
		{
			this.codeVersion = codeVersion;
		}
	}
}
