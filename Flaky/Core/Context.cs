using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal struct Context : IContext
	{
		private readonly ContextController controller;

		public long Sample { get; }

		public int SampleRate { get; private set; }

		internal TState GetOrCreateState<TState>(string id) where TState : class, new()
		{
			return controller.GetOrCreateState<TState>(id);
		}

		internal TFactory Get<TFactory>() where TFactory : class
		{
			return controller.Get<TFactory>();
		}

		internal Context(ContextController controller)
		{
			Sample = controller.Sample;
			SampleRate = controller.SampleRate;
			this.controller = controller;
		}
	}
}
