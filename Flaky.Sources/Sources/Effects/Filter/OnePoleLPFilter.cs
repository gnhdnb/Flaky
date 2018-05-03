namespace Flaky
{
	public class OnePoleLPFilter : OnePoleFilter
	{
		internal OnePoleLPFilter(Source source, Source cutoff, string id) : base(source, cutoff, id) { }

		internal OnePoleLPFilter(Source cutoff, string id) : base(cutoff, id) { }

		protected override Sample GetResult(Sample lp, Sample hp)
		{
			return lp;
		}
	}
}
