using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal interface IPipingSource 
	{
		void SetMainSource(Source mainSource);
	}

	public sealed class PipingSourceWrapper
	{
		private readonly IPipingSource pipingSource;

		internal PipingSourceWrapper(IPipingSource pipingSource) 
		{
			this.pipingSource = pipingSource;
		}

		internal void SetMainSource(Source mainSource) 
		{
			pipingSource.SetMainSource(mainSource);
		}

		internal Source Source 
		{
			get { return (Source)pipingSource; }
		}

		public static Source operator %(Source a, PipingSourceWrapper b)
		{
			b.SetMainSource(a);
			return b.Source;
		}

		public static Source operator %(float a, PipingSourceWrapper b)
		{
			return (Source)a % b;
		}
	}
}
