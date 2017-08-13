using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flaky
{
	public class Recorder : Source
	{
		private const int bufferSize = 44100;
		private readonly Source[] sources;
		private bool initialized = false;
		private State state;

		private class State : IDisposable
		{
			public List<IWaveWriter> Writers { get; private set; }

			public int Position { get; set; } = 0;

			public void Init(IWaveWriterFactory factory, IContext context, int channelsCount)
			{
				if (Writers == null)
					Writers = new List<IWaveWriter>();

				if (Writers.Count > channelsCount)
				{
					Writers.RemoveRange(Writers.Count - 1, channelsCount - Writers.Count);
				}

				if (Writers.Count < channelsCount)
					Writers.AddRange(
						Enumerable
							.Range(Writers.Count, channelsCount - Writers.Count)
							.Select(n => factory.Create($"flaky channel {n}", context.SampleRate)));
			}

			public void Dispose()
			{
				Writers?.ForEach(w => w?.Dispose());
			}
		}

		public Recorder(string id, params Source[] sources) : base(id)
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
			if (!initialized)
			{
				var factory = Get<IWaveWriterFactory>(context);

				state.Init(factory, context, sources.Length);

				initialized = true;
			}

			Sample result = 0;

			for(int channel = 0; channel < sources.Length; channel++)
			{
				var sample = sources[channel].Play(context);
				state.Writers[channel].Write(sample);
				result += sample;
			}

			return result;
		}
	}
}
