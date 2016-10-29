using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Pan : Source
	{
		private Source sound;
		private Source position;

		public Pan(Source sound, Source position)
		{
			this.sound = sound;
			this.position = position;
		}

		public override Sample Play(IContext context)
		{
			var positionValue = position.Play(context).Value;
			var soundValue = sound.Play(context);

			if (positionValue > 1)
				positionValue = 1;

			if (positionValue < -1)
				positionValue = -1;

			return new Sample
			{
				Left = soundValue.Left * (1 - positionValue),
				Right = soundValue.Right * (1 + positionValue)
			};
		}

		internal override void Initialize(IContext context)
		{
			Initialize(context, sound, position);
		}
	}
}
