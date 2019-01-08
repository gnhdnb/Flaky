using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Flaky.Tests")]
namespace Flaky
{
	public abstract class Source : ISource
	{
		private readonly string id;

		private Vector2 latestSample;
		private long latestSampleIndex = -1;
		private bool initialized = false;
		private bool hasMultipleParents = false;

		protected Source() { }

		protected Source(string id)
		{
			this.id = id;
		}

		public Vector2 Play(IContext context)
		{
			if (hasMultipleParents && context.Sample == latestSampleIndex)
				return latestSample;

			var result = NextSample(context);

			if (float.IsNaN(result.X))
				return new Vector2(0, 0);

			if (hasMultipleParents)
			{
				latestSample = result;
				latestSampleIndex = context.Sample;
			}

			return result;
		}

		protected abstract Vector2 NextSample(IContext context);

		public void Initialize(ISource parent, IContext context)
		{
			if (initialized)
				hasMultipleParents = true;

			initialized = true;
			Initialize(context);
		}

		protected abstract void Initialize(IContext context);

		protected TState GetOrCreate<TState>(IContext context) where TState : class, new()
		{
			return ((IFlakyContext)context).GetOrCreateState<TState>(id);
		}

		protected TFactory Get<TFactory>(IContext context) where TFactory : class
		{
			return ((IFlakyContext)context).Get<TFactory>();
		}

		protected void Initialize(IContext context, params ISource[] sources)
		{
			foreach (var source in sources)
			{
				if (source != null)
					source.Initialize(this, context);
			}
		}

		public abstract void Dispose();

		protected void Dispose(params ISource[] sources)
		{
			foreach (var source in sources)
			{
				if(source != null)
					source.Dispose();
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
	}
}
