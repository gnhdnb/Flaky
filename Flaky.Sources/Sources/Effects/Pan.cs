using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

		protected override Vector2 NextSample(IContext context)
		{
			var positionValue = position.Play(context).X;
			var widthValue = width.Play(context).X;
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

		internal static Vector2 Perform(Vector2 sound, float position, float width)
		{
			var average = (sound.X + sound.Y) / 2;

			return new Vector2
			{
				X = (sound.X * width + average * (1 - width)) * (1 - position),
				Y = (sound.Y * width + average * (1 - width)) * (1 + position)
			};
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.sound = mainSource;
		}
	}
}
