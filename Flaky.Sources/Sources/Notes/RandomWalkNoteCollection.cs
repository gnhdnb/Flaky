﻿using Redzen.Random.Double;
using System;

namespace Flaky
{
	internal class RandomWalkNoteCollection : NoteCollection
	{
		private readonly ZigguratGaussianDistribution distribution =
			new ZigguratGaussianDistribution(0, 1);

		private readonly ISource deviation;

		private float deviationValue = 0;

		public RandomWalkNoteCollection(string sequence, ISource deviation, string parentId) : base(sequence, parentId)
		{
			this.deviation = deviation ?? throw new ArgumentNullException();
		}

		protected override int GetNextNoteIndex(int currentNoteIndex)
		{
			if (sequence.Length <= 1)
				return 0;

			var newIndex = (int)Math.Round(
				currentNoteIndex +
				distribution.SampleStandard() * deviationValue)
				% sequence.Length;

			if (newIndex < 0)
				newIndex += sequence.Length;

			return newIndex;
		}

		public override void Update(IContext context)
		{
			base.Update(context);

			deviationValue = deviation.Play(context).X;
		}

		public override void Initialize(ISource parent, IFlakyContext context)
		{
			base.Initialize(parent, context);

			deviation.Initialize(parent, context);
		}

		public override void Dispose()
		{
			base.Dispose();

			deviation.Dispose();
		}
	}
}
