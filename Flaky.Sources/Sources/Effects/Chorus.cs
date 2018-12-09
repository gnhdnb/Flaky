using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Chr : Source, IPipingSource
	{
		private Source source;
		private State state;

		private class State
		{
			internal int sampleRate;
			internal int capacity;
			internal Vector2[] buffer;
			internal int position;
			internal long sample;
			internal LFO lfo1;
			internal LFO lfo2;
			internal LFO lfo3;
			internal LFO lfo4;

			internal void Initialize(IContext context)
			{
				if (buffer != null)
					return;

				sampleRate = context.SampleRate;
				capacity = sampleRate * 10;
				buffer = new Vector2[capacity];

				var random = new Random();

				lfo1 = new LFO(random.Next(330));
				lfo2 = new LFO(random.Next(330));
				lfo3 = new LFO(random.Next(330));
				lfo4 = new LFO(random.Next(330));
			}

		}

		protected override Vector2 NextSample(IContext context)
		{
			var sound = source.Play(context);

			var delta = context.Sample - state.sample;
			state.sample = context.Sample;
			state.position += (int)delta;

			while (state.position >= state.capacity)
				state.position -= state.capacity;

			state.buffer[state.position] = sound;

			state.lfo1.Step();
			state.lfo2.Step();
			state.lfo3.Step();
			state.lfo4.Step();

			var echo1 = Read(context, state, 440, 1, state.lfo1.Value);
			var echo2 = Read(context, state, 440, 2, state.lfo2.Value);
			var echo3 = Read(context, state, 440, 3, state.lfo3.Value);
			var echo4 = Read(context, state, 440, 4, state.lfo4.Value);

			return sound * 0.2f
				+ Pan.Perform(echo1, -1f, 1) * 0.2f
				+ Pan.Perform(echo2, 0.6f, 1) * 0.2f
				+ Pan.Perform(echo3, -0.6f, 1) * 0.2f
				+ Pan.Perform(echo4, 1f, 1) * 0.2f;
		}

		private Vector2 Read(IContext context, State state, long offset, int order, double finetune)
		{
			var roughPoint = (int)Math.Floor(finetune);
			var fineOffset = (float)(finetune - Math.Floor(finetune));

			var readPosition = state.position - offset * order + roughPoint;

			return 
				state.buffer[CorrectPosition(readPosition)] * (1 - fineOffset)
				+ state.buffer[CorrectPosition(readPosition + 1)] * fineOffset;
		}

		private long CorrectPosition(long position)
		{
			if (position < 0)
				position += state.capacity;

			if(position >= state.capacity)
				position -= state.capacity;

			return position;
		}

		internal Chr(string id) : base(id) { }

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			state.Initialize(context);

			Initialize(context, source);
		}

		public override void Dispose()
		{
			Dispose(source);
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}

		internal class LFO
		{
			private double value = 0;
			private int amount;
			private double delta = 0.009f;

			public double Value
			{
				get { return value; }
			}

			public LFO(int amount)
			{
				this.amount = amount;
			}

			public void Step()
			{
				value += delta;

				if(value >= amount || value <= -amount)
				{
					delta = -delta;
				}
			}
		}
	}
}
