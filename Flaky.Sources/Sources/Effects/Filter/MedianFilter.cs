using System;
using System.Numerics;

namespace Flaky
{
	public class MedianFilter : Source, IPipingSource
	{
		private Source cutoff;
		private Source mainSource;
		private State state;

		private class State
		{
			public const int maxWindow = 512;
			public Vector2[] buffer = new Vector2[maxWindow];
			public float[] sortingBuffer = new float[maxWindow];
			public int windowStart = 0;
		}

		public MedianFilter(Source cutoff, string id) : base(id)
		{
			this.cutoff = cutoff;
		}

		public void SetMainSource(Source mainSource)
		{
			this.mainSource = mainSource;
		}

		protected override Vector2 NextSample(IContext context)
		{
			var input = mainSource.Play(context);
			var cutoffValue = cutoff.Play(context).X;

			if (cutoffValue < 0)
				cutoffValue = 0;

			if (cutoffValue > 1)
				cutoffValue = 1;

			state.buffer[state.windowStart] = input;

			state.windowStart++;
			state.windowStart = state.windowStart % State.maxWindow;

			var windowSize = (int)(State.maxWindow * (1 - cutoffValue));

			if (windowSize < 2)
				return new Vector2(input.X, input.X);


			var windowEnd = state.windowStart - windowSize;

			int counter = 0;
			for(int i = windowEnd; i < state.windowStart; i++)
			{
				var index = i;

				if (index < 0)
					index += State.maxWindow;

				state.sortingBuffer[counter] = state.buffer[index].X;

				counter++;
			}

			Array.Sort(state.sortingBuffer, 0, windowSize);

			var value = state.sortingBuffer[windowSize / 2];

			return new Vector2(
				value,
				value);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			Initialize(context, cutoff, mainSource);
		}

		public override void Dispose()
		{
			Dispose(cutoff, mainSource);
		}
	}
}
