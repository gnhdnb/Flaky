using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Pan : Source, IPipingSource
	{
		private Source sound;
		private Source position;
		private Source width;

		internal Pan(Source position) : this(position, 1.0f) { }

		internal Pan(Source position, Source width)
		{
			this.position = position;
			this.width = width;
		}

		protected override Sample NextSample(IContext context)
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

			return Perform(soundValue, positionValue, widthValue);
		}

		public override void Initialize(IContext context)
		{
			Initialize(context, sound, position, width);
		}

		public override void Dispose()
		{
			Dispose(sound, position, width);
		}

		internal static Sample Perform(Sample sound, float position, float width)
		{
			var average = (sound.Left + sound.Right) / 2;

			return new Sample
			{
				Left = (sound.Left * width + average * (1 - width)) * (1 - position),
				Right = (sound.Right * width + average * (1 - width)) * (1 + position)
			};
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.sound = mainSource;
		}
	}
}
