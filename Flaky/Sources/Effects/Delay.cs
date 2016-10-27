using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Delay : Source
	{
		private readonly Source time;
		private readonly Source sound;

		private class State
		{
			internal readonly int sampleRate = 44100;
			internal readonly int capacity;
			internal readonly float[] buffer;
			internal int position;
			internal long sample;

			public State()
			{
				capacity = sampleRate * 10;
				buffer = new float[capacity];
			}
		}

		public Delay(Source sound, Source time)
		{
			this.time = time;
			this.sound = sound;
		}

		public Delay(Source sound, Source time, string id) : base(id)
		{
			this.time = time;
			this.sound = sound;
		}

		public override Sample Play(IContext context)
		{
			var state = GetOrCreate<State>(context);

			var soundValue = sound.Play(context).Value;

			var delta = context.Sample - state.sample;
			state.sample = context.Sample;

			state.position += (int)delta;

			while (state.position >= state.capacity)
				state.position -= state.capacity;

			var writePosition = GetWritePosition(context, state);

			var result = state.buffer[state.position] / 2 + soundValue;

			state.buffer[writePosition] = result;

			return new Sample { Value = result };
		}

		private int GetWritePosition(IContext context, State state)
		{
			var timeValue = (int)(time.Play(context).Value * state.sampleRate);

			if (timeValue <= 0)
				return state.position;
			if (timeValue >= state.capacity)
				timeValue = state.capacity - 2;

			var writePosition = state.position + timeValue;

			while (writePosition >= state.capacity)
				writePosition -= state.capacity;

			return writePosition;
		}

		internal override void Initialize(IContext context)
		{
			Initialize(context, time, sound);
		}
	}
}
