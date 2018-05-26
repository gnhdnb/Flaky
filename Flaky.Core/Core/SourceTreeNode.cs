using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class SourceTreeNode
	{
		ISource root;
		List<SourceTreeNode> children = new List<SourceTreeNode>();
		int subtree = 0;

		internal SourceTreeNode(ISource root)
		{
			this.root = root;
		}

		internal void AddConnection(ISource source)
		{
			children.Add(new SourceTreeNode(source));
		}

		internal void AddConnection(SourceTreeNode node)
		{
			children.Add(node);
		}

		internal SourceTreeNode FindNodeFor(ISource source)
		{
			if (root == source)
				return this;

			return children
				.Select(n => n.FindNodeFor(source))
				.Distinct()
				.SingleOrDefault(n => n != null);
		}

		internal void SetSubtree(int subtree)
		{
			if (subtree == 0 || this.subtree == 0)
			{
				this.subtree = subtree;
				children.ForEach(c => c.SetSubtree(subtree));
			}
		}

		internal ISource Root { get { return root; } }

		public int Subtree { get { return subtree; } }

		public int GetOrder()
		{
			if (children.Any())
				return children.Max(c => c.GetOrder()) + 1;
			else
				return 1;
		}

		internal int GetWeight()
		{
			return GetWeight(new HashSet<SourceTreeNode>());
		}

		private int GetWeight(HashSet<SourceTreeNode> excludingNodes)
		{
			return GetWeight(excludingNodes, new HashSet<SourceTreeNode>());
		}

		private int GetWeight(
			HashSet<SourceTreeNode> excludingNodes,
			HashSet<SourceTreeNode> visitedNodes)
		{
			visitedNodes.Add(this);

			return 1 + children
				.Where(c => !excludingNodes.Contains(c))
				.Where(c => !visitedNodes.Contains(c))
				.Sum(c => c.GetWeight(excludingNodes, visitedNodes));
		}

		internal List<SourceTreeNode> Split(int subtreesCount)
		{
			var balancedSubtreeWeight = GetWeight() / subtreesCount + 1;

			var excludingNodes = new HashSet<SourceTreeNode>();
			var result = new List<SourceTreeNode>();

			SetSubtree(0);

			for(int i = 1; i < subtreesCount; i++)
			{
				var subtree = GetOptimalSubtree(balancedSubtreeWeight, excludingNodes);

				if (subtree != null)
					subtree.SetSubtree(i);

				if (subtree == null)
					return Split(i);

				excludingNodes.Add(subtree);
				result.Add(subtree);

				balancedSubtreeWeight =
					(GetWeight() - excludingNodes.Sum(n => n.GetWeight())) / (subtreesCount - i) + 1;
			}

			return result;
		}

		private bool ContainsAnyOf(HashSet<SourceTreeNode> excludingNodes)
		{
			if (excludingNodes.Contains(this))
				return true;

			return children.Any(c => c.ContainsAnyOf(excludingNodes));
		}

		public void Enumerate(Action<SourceTreeNode, SourceTreeNode> action)
		{
			foreach(var child in children)
			{
				action(this, child);
				child.Enumerate(action);
			}
		}

		private SourceTreeNode GetOptimalSubtree(
			int optimalWeight, 
			HashSet<SourceTreeNode> excludingNodes)
		{
			//if (optimalWeight > GetWeight(excludingNodes) && !ContainsAnyOf(excludingNodes))
			if (optimalWeight > GetWeight(excludingNodes))
				return this;

			return children
				.Where(c => !excludingNodes.Contains(c))
				.Select(c => c.GetOptimalSubtree(optimalWeight, excludingNodes))
				.Where(s => s != null)
				//.Where(s => !s.ContainsAnyOf(excludingNodes))
				.OrderByDescending(c => c.GetWeight(excludingNodes))
				.FirstOrDefault();
		}

		public override string ToString()
		{
			return root.ToString();
		}
	}
}
