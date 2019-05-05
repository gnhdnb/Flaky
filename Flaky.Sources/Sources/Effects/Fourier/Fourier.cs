using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flaky
{
	public class Fourier : FrequencyDomainOperator
	{
		private float effect;
		private bool reverse = false;

		public Fourier(float effect, int oversampling, string id) 
			: base(oversampling, id)
		{
			if (effect < -1)
				effect = -1;

			if (effect < 0)
			{
				this.reverse = true;
				effect = -effect;
			}

			if (effect > 1)
				effect = 1;

			this.effect = effect;
		}

		protected override void Processor(float[] left, float[] right, float effect)
		{
			var framesCount = left.Length;

			var leftThreshold = ThresholdPower(left);
			var rightThreshold = ThresholdPower(right);

			for (int i = 0; i < framesCount; i++)
			{
				if (Power(left[i]) >= leftThreshold)
					left[i] = 0;

				if (Power(right[i]) >= rightThreshold)
					right[i] = 0;
			}
		}

		private float ThresholdPower(float[] buffer)
		{
			var framesCount = buffer.Length;

			var thresholdIndex = (int)Math.Floor((framesCount - 1) * effect);

			float[] orderedPower = buffer
				.Select(v => Power(v))
				.OrderBy(v => v)
				.ToArray();

			return orderedPower[framesCount - thresholdIndex - 1];

			var average = orderedPower.Average();

			for (int i = 0; i < framesCount; i++)
			{
				if (orderedPower[i] > average)
					return orderedPower[i];
			}

			return orderedPower.Last();
		}

		private float Power(float value)
		{
			return reverse ? -Math.Abs(value) : Math.Abs(value);
		}

		protected override float GetEffect(IContext context)
		{
			return effect;
		}
	}
}
