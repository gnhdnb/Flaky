using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public abstract class Source : ISource
	{
		private readonly string id;
		private IStateContainer stateContainer;

		protected Source() { }

		protected Source(string id)
		{
			this.id = id;
		}

		public abstract Sample Play(IContext context);

		public abstract void Initialize(IContext context);

		protected TState GetOrCreate<TState>(IContext context) where TState : class, new()
		{
			if (stateContainer == null)
			{
				if (id == null)
					stateContainer = new StateContainer<TState>(new TState());
				else
					stateContainer = new StateContainer<TState>(((IFlakyContext)context).GetOrCreateState<TState>(id));
			}

			return ((StateContainer<TState>)stateContainer).State;
		}

		protected TFactory Get<TFactory>(IContext context) where TFactory : class
		{
			return ((IFlakyContext)context).Get<TFactory>();
		}

		protected void Initialize(IContext context, params Source[] sources)
		{
			foreach (var source in sources)
			{
				source.Initialize(context);
			}
		}

		public static implicit operator Source(float d)
		{
			return new Constant(d);
		}

		public static implicit operator Source(int d)
		{
			return new Constant(d);
		}

		public static implicit operator Source(double d)
		{
			return new Constant((float)d);
		}

		public static Source operator +(Source a, Source b)
		{
			return new Sum(a, b);
		}

		public static Source operator -(Source a, Source b)
		{
			return new Difference(a, b);
		}

		public static Source operator *(Source a, Source b)
		{
			return new Multiply(a, b);
		}

		private interface IStateContainer { }
		private class StateContainer<TState> : IStateContainer where TState : class, new()
		{
			internal TState State { get; }

			internal StateContainer(TState state)
			{
				State = state;
			}
		}
	}
}
