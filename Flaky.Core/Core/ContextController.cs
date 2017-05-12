﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal class ContextController
	{
		private readonly Configuration configuration;
		private readonly Dictionary<StateKey, object> states = new Dictionary<StateKey, object>();
		private long sample;

		internal ContextController(int sampleRate, int bpm, Configuration configuration)
		{
			SampleRate = sampleRate;
			this.BPM = bpm;
			this.configuration = configuration;
		}

		internal int SampleRate { get; private set; }

		internal int BPM { get; private set; }

		internal int Beat { get; private set; }

		internal long Sample { get { return sample; } }

		internal TState GetOrCreateState<TState>(string id) where TState : class, new()
		{
			var key = new StateKey(typeof(TState), id);

			if (!states.ContainsKey(key))
				states[key] = new TState();

			return (TState)states[key];
		}

		internal TFactory Get<TFactory>() where TFactory : class
		{
			return configuration.Get<TFactory>();
		}

		internal void NextSample()
		{
			sample++;

			Beat = (int)Math.Floor((sample / (double)SampleRate) * ((double)BPM / 60));
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
