using System.Numerics;

namespace Flaky
{
	public class OnePoleHPFilter : OnePoleFilter
	{
		internal OnePoleHPFilter(Source source, Source cutoff, string id) : base(source, cutoff, id) { }
		internal OnePoleHPFilter(Source cutoff, string id) : base(cutoff, id) { }

		protected override Vector2 GetResult(Vector2 lp, Vector2 hp)
		{
			return hp;
		}
	}
}
