using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal interface IStateContainer
	{

	}

	internal class StateContainer<TState> : IStateContainer where TState : class, new()
	{
		internal TState State { get; set; }

		internal StateContainer(TState state) 
		{
			State = state;
		}
	}

	public abstract class Source
	{
		private readonly string id;
		private IStateContainer StateContainer { get; set; }

		protected Source() { }

		protected Source(string id)
		{
			this.id = id;
		}

		internal TState GetOrCreate<TState>(IContext context) where TState : class, new()
		{
			if (StateContainer == null)
			{
				if (id == null)
					StateContainer = new StateContainer<TState>(new TState());
				else
					StateContainer = new StateContainer<TState>(((Context)context).GetOrCreateState<TState>(id));
			}

			return ((StateContainer<TState>)StateContainer).State;
		}

		public abstract Sample Play(IContext context);


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
	}
}
