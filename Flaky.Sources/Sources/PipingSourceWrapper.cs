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

	public class PipingSourceWrapper<T> where T: Source
	{
		private readonly IPipingSource<T> pipingSource;

		internal PipingSourceWrapper(IPipingSource<T> pipingSource) 
		{
			this.pipingSource = pipingSource;
		}

		internal void SetMainSource(T mainSource) 
		{
			pipingSource.SetMainSource(mainSource);
		}

		internal Source Source 
		{
			get { return (Source)pipingSource; }
		}

		public static Source operator %(T a, PipingSourceWrapper<T> b)
		{
			b.SetMainSource(a);
			return b.Source;
		}
	}

	public class PipingSourceWrapper : PipingSourceWrapper<Source>
	{
		internal PipingSourceWrapper(IPipingSource pipingSource) : base(pipingSource) { }

		public static Source operator %(float a, PipingSourceWrapper b)
		{
			return (Source)a % b;
		}
	}
}
