using GraphX.Controls;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class SourceTreeArea : GraphArea
		<SourceVertex, SourceTreeEdge, BidirectionalGraph<SourceVertex, SourceTreeEdge>> { }
}
