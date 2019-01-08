using System.Numerics;

namespace Flaky
{
	public class OnePoleLPFilter : OnePoleFilter
	{
		internal OnePoleLPFilter(Source source, Source cutoff, string id) 
			: base(source, cutoff, false, id) { }

		internal OnePoleLPFilter(Source cutoff, string id) 
			: base(cutoff, false, id) { }
	}
}
