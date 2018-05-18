using GraphX.PCL.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class SourceTreeEdge : EdgeBase<SourceVertex>
	{
		public SourceTreeEdge(SourceVertex source, SourceVertex target) 
			: base(source, target, 1)
		{
		}
	}
}
