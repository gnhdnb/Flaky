using System;
using System.Collections.Generic;
using System.Text;

namespace Flaky
{
	public class FourierLP : FrequencyDomainOperator
	{
		private Source effect;

		internal FourierLP(Source effect, int oversampling, string id) : base(oversampling, id)
		{
			this.effect = effect;
		}

		protected override float GetEffect(IContext context)
		{
			return effect.Play(context).X;
		}

		protected override void Processor(float[] left, float[] right, float effect)
		{
			if (effect < 0)
				effect = 0;

			if (effect > 1)
				effect = 1;

			var framesCount = left.Length;
			var lpIndex = (int)Math.Floor((framesCount - 1) * effect);

			for(int i = 0; i < framesCount; i++)
			{
				var pw = Math.Abs(i - lpIndex) < 20 ? 1 : 0;

				left[i] = Math.Abs(left[i] * pw);
				right[i] = Math.Abs(right[i] * pw);
			}
		}

		protected override void Initialize(IContext context)
		{
			base.Initialize(context);

			Initialize(context, effect);
		}
	}
}
