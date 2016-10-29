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
		private State state;

		private class State
		{
			internal int sampleRate;
			internal int capacity;
			internal Sample[] buffer;
			internal int position;
			internal long sample;

			internal void Initialize(IContext context)
			{
				if (buffer != null)
					return;

				sampleRate = context.SampleRate;
				capacity = sampleRate * 10;
				buffer = new Sample[capacity];
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
			var soundValue = sound.Play(context);

			var delta = context.Sample - state.sample;
			state.sample = context.Sample;

			state.position += (int)delta;

			while (state.position >= state.capacity)
				state.position -= state.capacity;

			var writePosition = GetWritePosition(context, state);

			var delay = state.buffer[state.position];

			var result = new Sample
			{
				Left = delay.Left / 2 + soundValue.Left,
				Right = delay.Right / 2 + soundValue.Right
			};

			state.buffer[writePosition] = result;

			return result;
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
			state = GetOrCreate<State>(context);

			state.Initialize(context);

			Initialize(context, time, sound);
		}
	}
}
