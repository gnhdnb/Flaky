using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal interface IPipingSource<T> where T : Source
	{
		void SetMainSource(T mainSource);
	}

	internal interface IPipingSource : IPipingSource<Source> { }

	public class PipingSourceWrapper<TSource, TResult> where TSource: Source where TResult: Source
	{
		private readonly IPipingSource<TSource> pipingSource;

		internal PipingSourceWrapper(IPipingSource<TSource> pipingSource) 
		{
			this.pipingSource = pipingSource;
		}

		internal void SetMainSource(TSource mainSource) 
		{
			pipingSource.SetMainSource(mainSource);
		}

		internal TResult Source 
		{
			get { return (TResult)pipingSource; }
		}

		public static TResult operator %(TSource a, PipingSourceWrapper<TSource, TResult> b)
		{
			b.SetMainSource(a);
			return b.Source;
		}
	}

	public class PipingSourceWrapper : PipingSourceWrapper<Source, Source>
	{
		internal PipingSourceWrapper(IPipingSource pipingSource) : base(pipingSource) { }

		public static Source operator %(float a, PipingSourceWrapper b)
		{
			return (Source)a % b;
		}
	}
}
