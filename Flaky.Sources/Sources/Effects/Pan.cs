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
		private Source width;

		public Pan(Source sound, Source position) : this(sound, position, 1.0f) { }

		public Pan(Source sound, Source position, Source width)
		{
			this.sound = sound;
			this.position = position;
			this.width = width;
		}

		public override Sample Play(IContext context)
		{
			var positionValue = position.Play(context).Value;
			var widthValue = width.Play(context).Value;
			var soundValue = sound.Play(context);

			if (positionValue > 1)
				positionValue = 1;

			if (positionValue < -1)
				positionValue = -1;

			if (widthValue < 0)
				widthValue = 0;

			if (widthValue > 2)
				widthValue = 2;

			var average = (soundValue.Left + soundValue.Right) / 2;

			return new Sample
			{
				Left = (soundValue.Left * widthValue + average * (1 - widthValue)) * (1 - positionValue),
				Right = (soundValue.Right * widthValue + average * (1 - widthValue)) * (1 + positionValue)
			};
		}

		public override void Initialize(IContext context)
		{
			Initialize(context, sound, position);
		}
	}
}
