using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flaky.Sources
{
	public class Recorder : Source
	{
		private readonly Source[] sources;
		private State state;
		private Thread writer;

		private class State
		{
			public List<Sample[]> Buffers { get; private set; }

			public int Position { get; set; } = 0;

			public void Init(int channelsCount)
			{
				if(Buffers != null)
				{
					Buffers.RemoveRange()
				}
			}
		}

		public Recorder(params Source[] sources)
		{
			this.sources = sources;
		}

		public override void Dispose()
		{
			Dispose(sources);
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			Initialize(context, sources);
		}

		public override Sample Play(IContext context)
		{
			
		}
	}
}
