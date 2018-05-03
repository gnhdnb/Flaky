namespace Flaky
{
	public class OnePoleHPFilter : OnePoleFilter
	{
		internal OnePoleHPFilter(Source source, Source cutoff, string id) : base(source, cutoff, id) { }
		internal OnePoleHPFilter(Source cutoff, string id) : base(cutoff, id) { }

		protected override Sample GetResult(Sample lp, Sample hp)
		{
			return hp;
		}
	}
}
