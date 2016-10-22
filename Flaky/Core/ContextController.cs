using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal class ContextController
	{
		private long sample;
		private readonly Dictionary<StateKey, object> states = new Dictionary<StateKey, object>();

		internal long Sample { get { return sample; } }

		internal TState GetOrCreateState<TState>(string id) where TState : class, new()
		{
			var key = new StateKey(typeof(TState), id);

			if (!states.ContainsKey(key))
				states[key] = new TState();

			return (TState)states[key];
		}

		internal void NextSample()
		{
			sample++;
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
