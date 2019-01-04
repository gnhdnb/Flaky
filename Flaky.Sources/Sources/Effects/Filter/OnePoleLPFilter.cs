using System.Numerics;

namespace Flaky
{
	public class OnePoleLPFilter : OnePoleFilter
	{
		internal OnePoleLPFilter(Source source, Source cutoff, string id) : base(source, cutoff, id) { }

		internal OnePoleLPFilter(Source cutoff, string id) : base(cutoff, id) { }

		protected override Vector2 GetResult(Vector2 lp, Vector2 hp)
		{
			return lp;
		}
	}
}
