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

		public int Subtree { get { return subtree; } }

		public int GetOrder()
		{
			if (children.Any())
				return children.Max(c => c.GetOrder()) + 1;
			else
				return 1;
		}

		public void Enumerate(Action<SourceTreeNode, SourceTreeNode> action)
		{
			foreach (var child in children)
			{
				action(this, child);
				child.Enumerate(action);
			}
		}

		internal List<SourceTreeNode> GetJunctions()
		{
			return children
				.Where(c => c.Subtree != Subtree)
				.Concat(children.SelectMany(c => c.GetJunctions()))
				.ToList();
		}

		internal List<SourceTreeNode> Split(int subtreesCount, Func<ISource, bool> filter)
		{
			var balancedSubtreeWeight = GetWeight() / subtreesCount + 1;

			var excludingNodes = new HashSet<SourceTreeNode>();
			var result = new List<SourceTreeNode>();

			SetSubtree(0);

			for (int i = 1; i < subtreesCount; i++)
			{
				var subtree = GetOptimalSubtree(balancedSubtreeWeight, excludingNodes, filter);

				if (subtree != null)
					subtree.SetSubtree(i);

				if (subtree == null)
					return Split(i, filter);

				excludingNodes.Add(subtree);
				result.Add(subtree);

				balancedSubtreeWeight =
					(GetWeight() - excludingNodes.Sum(n => n.GetWeight())) / (subtreesCount - i) + 1;
			}

			return result;
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

		internal ISource Source { get { return root; } }

		private void SetSubtree(int subtree)
		{
			if (subtree == 0 || this.subtree == 0)
			{
				this.subtree = subtree;
				children.ForEach(c => c.SetSubtree(subtree));
			}
		}

		private int GetWeight()
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

		private bool ContainsAnyOf(HashSet<SourceTreeNode> excludingNodes)
		{
			if (excludingNodes.Contains(this))
				return true;

			return children.Any(c => c.ContainsAnyOf(excludingNodes));
		}

		private SourceTreeNode GetOptimalSubtree(
			int optimalWeight, 
			HashSet<SourceTreeNode> excludingNodes,
			Func<ISource, bool> filter)
		{
			if (optimalWeight > GetWeight(excludingNodes))
				return this;

			return children
				.Where(c => !excludingNodes.Contains(c))
				.Select(c => c.GetOptimalSubtree(optimalWeight, excludingNodes, filter))
				.Where(s => s != null)
				.Where(s => filter(s.root))
				.OrderByDescending(c => c.GetWeight(excludingNodes))
				.FirstOrDefault();
		}

		public override string ToString()
		{
			return root.ToString();
		}
	}
}
