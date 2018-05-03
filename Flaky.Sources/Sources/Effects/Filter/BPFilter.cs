namespace Flaky
{
	public class BPFilter : MultiPoleFilter
	{
		internal BPFilter(Source cutoff, Source resonance, string id) : base(cutoff, resonance, id)
		{
		}

		protected override Source CreateFilterChain(Source input, Source cutoff, string id)
		{
			Source filterChain = new OnePoleLPFilter(input, cutoff, $"{id}_lp1");
			filterChain = new OnePoleHPFilter(filterChain, cutoff, $"{id}_hp2");
			filterChain = new OnePoleLPFilter(filterChain, cutoff, $"{id}_lp3");
			filterChain = new OnePoleHPFilter(filterChain, cutoff, $"{id}_hp4");

			return filterChain;
		}
	}
}
