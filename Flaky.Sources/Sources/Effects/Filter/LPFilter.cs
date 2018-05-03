using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class LPFilter : MultiPoleFilter
	{
		internal LPFilter(Source cutoff, Source resonance, string id) : base(cutoff, resonance, id)
		{
		}

		protected override Source CreateFilterChain(Source input, Source cutoff, string id)
		{
			Source filterChain = new OnePoleLPFilter(input, cutoff, $"{id}_lp1");
			filterChain = new OnePoleLPFilter(filterChain, cutoff, $"{id}_lp2");
			filterChain = new OnePoleLPFilter(filterChain, cutoff, $"{id}_lp3");
			filterChain = new OnePoleLPFilter(filterChain, cutoff, $"{id}_lp4");

			return filterChain;
		}
	}
}
