using Redzen.Random.Double;
using System;

namespace Flaky
{
	public class RandomWalkSequence : Sequence
	{
		private readonly ZigguratGaussianDistribution distribution =
			new ZigguratGaussianDistribution(0, 1);

		private readonly Source deviation;

		private float deviationValue = 0;

		public RandomWalkSequence(string sequence, Source deviation, Source length, string id) 
			: base(sequence, length, id)
		{
			this.deviation = deviation;
		}

		public RandomWalkSequence(string sequence, Source deviation, int size, string id) 
			: base(sequence, size, id)
		{
			this.deviation = deviation;
		}

		public RandomWalkSequence(string sequence, Source deviation, Source length, bool skipSilentNotes, string id) 
			: base(sequence, length, id)
		{
			SkipSilentNotes = skipSilentNotes;

			this.deviation = deviation;
		}

		protected override int GetNextNoteIndex(IContext context, State state)
		{
			if (notes.Length <= 1)
				return 0;

			var newIndex = (int)Math.Round(
				state.index + 
				distribution.SampleStandard() * deviationValue)
				% notes.Length;

			if (newIndex < 0)
				newIndex += notes.Length;

			return newIndex;
		}

		protected override void Update(IContext context)
		{
			deviationValue = deviation.Play(context).Value;
		}
	}
}
