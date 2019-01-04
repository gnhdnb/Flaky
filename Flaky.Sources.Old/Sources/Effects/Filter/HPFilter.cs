namespace Flaky
{
	public class HPFilter : MultiPoleFilter
	{
		internal HPFilter(Source cutoff, Source resonance, string id) : base(cutoff, resonance, id)
		{
		}

		protected override Source CreateFilterChain(Source input, Source cutoff, string id)
		{
			Source filterChain = new OnePoleHPFilter(input, cutoff, $"{id}_hp1");
			filterChain = new OnePoleHPFilter(filterChain, cutoff, $"{id}_hp2");
			filterChain = new OnePoleHPFilter(filterChain, cutoff, $"{id}_hp3");
			filterChain = new OnePoleHPFilter(filterChain, cutoff, $"{id}_hp4");

			return filterChain;
		}
	}
}
