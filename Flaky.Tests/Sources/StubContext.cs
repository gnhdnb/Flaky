using System;
using System.Collections.Generic;

namespace Flaky.Tests
{
	internal class StubContext : IFlakyContext
	{
		private int beat;

		public long Sample { get; set; }

		public DateTime Timestamp { get; set; }

		public int SampleRate => 44100;

		public int Beat
		{
			get
			{
				return beat;
			}

			set
			{
				beat = value;
				Sample = value * SampleRate * 60 / BPM;
				MetronomeTick = true;
			}
		}

		public bool MetronomeTick { get; set; }

		public int BPM { get; set; } = 120;

		public TFactory Get<TFactory>() where TFactory : class
		{
			throw new NotImplementedException();
		}

		public TState GetOrCreateState<TState>(string id) where TState : class, new()
		{
			var key = (typeof(TState), id);

			if (!states.ContainsKey(key))
				states[key] = new TState();

			return (TState)states[key];
		}

		public void ShowError(string error)
		{
			// do nothing
		}

		private readonly Dictionary<(Type, string), object> states
			= new Dictionary<(Type, string), object>();
	}
}
