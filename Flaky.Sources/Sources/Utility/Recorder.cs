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
		private const int bufferSize = 44100;
		private readonly Source[] sources;
		private State state;
		private Thread writer;

		private class State
		{
			public List<Sample[]> Buffers { get; private set; }

			public int Position { get; set; } = 0;

			public void Init(int channelsCount)
			{
				if (Buffers == null)
					Buffers = new List<Sample[]>();

				if (Buffers.Count > channelsCount)
					Buffers.RemoveRange(Buffers.Count - 1, channelsCount - Buffers.Count);

				if (Buffers.Count < channelsCount)
					Buffers.AddRange(
						Enumerable
							.Range(0, channelsCount - Buffers.Count)
							.Select(n => new Sample[bufferSize]));
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
			state.Init(sources.Length);

			for(int channel = 0; channel < sources.Length; channel++)
			{
				var sample = sources[channel].Play(context);

				state.Buffers[channel][state.Position] = sample;
				state.Position++;
			}
		}
	}
}
