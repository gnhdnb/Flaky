using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public abstract class Source : IFlakySource
	{
		private readonly string id;
		private IStateContainer stateContainer;

		private Sample latestSample;
		private long latestSampleIndex = -1;
		private IExternalSourceProcessor exteralProcessor;

		protected Source() { }

		protected Source(string id)
		{
			this.id = id;
		}

		Sample IFlakySource.PlayInCurrentThread(IContext context)
		{
			return PlayInCurrentThread(context);
		}

		internal Sample PlayInCurrentThread(IContext context)
		{
			if (context.Sample == latestSampleIndex)
				return latestSample;

			var result = NextSample(context);

			latestSample = result;
			latestSampleIndex = context.Sample;

			return result;
		}

		public Sample Play(IContext context)
		{
			if (exteralProcessor != null)
				return exteralProcessor.Play(context);

			return PlayInCurrentThread(context);
		}

		protected abstract Sample NextSample(IContext context);

		void ISource.Init(IContext context)
		{
			Initialize(context);
		}

		protected abstract void Initialize(IContext context);

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
			var flakyContext = context as IFlakyContext;

			foreach (var source in sources)
			{
				if (source != null)
				{
					flakyContext.RegisterConnection(source, this);

					if (flakyContext.AlreadyInitialized(source))
						continue;

					flakyContext.RegisterInitialization(source);
					source.Initialize(context);
				}
			}
		}

		public abstract void Dispose();

		protected void Dispose(params Source[] sources)
		{
			foreach (var source in sources)
			{
				if(source != null)
					source.Dispose();
			}
		}

		public override string ToString()
		{
			if (id != null)
				return $"{GetType().Name}({id})";
			else
				return $"{GetType().Name}";
		}

		void IFlakySource.SetExternalProcessor(IExternalSourceProcessor processor)
		{
			this.exteralProcessor = processor;
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
