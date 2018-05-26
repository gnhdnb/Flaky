using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal class ContextController : IDisposable
	{
		private readonly Configuration configuration;
		private readonly bool isCloned = false;
		private readonly Dictionary<StateKey, object> states = new Dictionary<StateKey, object>();
		private readonly Dictionary<StateKey, int> versions = new Dictionary<StateKey, int>();
		private long sample;

		private ContextController(ContextController other)
		{
			this.configuration = other.configuration;
			this.states = other.states;
			this.versions = other.versions;
			this.sample = other.sample;
			this.MetronomeTick = other.MetronomeTick;
			this.Beat = other.Beat;
			this.BPM = other.BPM;
			this.CodeVersion = other.CodeVersion;
			this.isCloned = true;
		}

		internal ContextController(int sampleRate, int bpm, Configuration configuration)
		{
			SampleRate = sampleRate;
			this.BPM = bpm;
			this.configuration = configuration;
		}

		internal int SampleRate { get; private set; }

		internal int BPM { get; private set; }

		internal int Beat { get; private set; }
		internal bool MetronomeTick { get; private set; } = true;

		internal long Sample { get { return sample; } }

		internal int CodeVersion { get; set; }

		internal TState GetOrCreateState<TState>(string id, int codeVersion) where TState : class, new()
		{
			var key = new StateKey(typeof(TState), id);

			if (!states.ContainsKey(key) || versions[key] < codeVersion - 1)
				states[key] = new TState();

			versions[key] = codeVersion;

			return (TState)states[key];
		}

		internal TFactory Get<TFactory>() where TFactory : class
		{
			return configuration.Get<TFactory>();
		}

		internal void NextSample()
		{
			sample++;

			MetronomeTick = (sample * BPM) % (SampleRate * 60) == 0;
			Beat = (int)((sample * BPM) / (SampleRate * 60));
		}

		internal ContextController Clone()
		{
			return new ContextController(this);
		}

		public void Dispose()
		{
			foreach(var state in states)
			{
				if(state.Value is IDisposable && state.Value != null)
				{
					(state.Value as IDisposable).Dispose();
				}
			}
		}

		private struct StateKey
		{
			public readonly Type Type;
			public readonly string Id;

			public StateKey(Type type, string id)
			{
				Type = type;
				Id = id;
			}
		}
	}
}
