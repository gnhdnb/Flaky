using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal struct Context : IFlakyContext
	{
		private readonly ContextController controller;
		private readonly int codeVersion;
		private SourceTreeNode sourceTreeRoot;
		private HashSet<ISource> initializedSources;

		public long Sample { get; }

		public int Beat { get; }
		public bool MetronomeTick { get; }

		public int BPM { get; }

		public int SampleRate { get; private set; }

		TState IFlakyContext.GetOrCreateState<TState>(string id)
		{
			return controller.GetOrCreateState<TState>(id, codeVersion);
		}

		TFactory IFlakyContext.Get<TFactory>()
		{
			return controller.Get<TFactory>();
		}

		void IFlakyContext.RegisterConnection(ISource from, ISource to)
		{
			var existingSourceNode = sourceTreeRoot.FindNodeFor(from);
			var destinationNode = sourceTreeRoot.FindNodeFor(to);

			if (existingSourceNode == null)
				destinationNode.AddConnection(from);
			else
				destinationNode.AddConnection(existingSourceNode);
		}

		void IFlakyContext.RegisterInitialization(ISource source)
		{
			initializedSources.Add(source);
		}

		bool IFlakyContext.AlreadyInitialized(ISource source)
		{
			return initializedSources.Contains(source);
		}

		internal Context(ContextController controller)
		{
			sourceTreeRoot = null;
			initializedSources = null;
			Sample = controller.Sample;
			Beat = controller.Beat;
			BPM = controller.BPM;
			SampleRate = controller.SampleRate;
			MetronomeTick = controller.MetronomeTick;
			codeVersion = -1;
			this.controller = controller;
		}

		internal Context(ContextController controller, int codeVersion, ISource root) : this(controller)
		{
			sourceTreeRoot = new SourceTreeNode(root);
			initializedSources = new HashSet<ISource>();

			this.codeVersion = codeVersion;
		}

		internal SourceTreeNode SourceTreeRoot { get { return sourceTreeRoot; } }
	}
}
