using GraphX.PCL.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class SourceVertex : VertexBase
	{
		private readonly SourceTreeNode node;

		public SourceVertex(SourceTreeNode node)
		{
			this.node = node;
		}

		public SourceTreeNode Node { get { return node; } }

		public override string ToString()
		{
			return node.ToString();
		}
	}
}
