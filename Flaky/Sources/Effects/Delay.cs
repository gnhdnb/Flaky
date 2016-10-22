using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Delay : Source
	{
		private readonly Source time;
		private readonly Source sound;
		private readonly float[] buffer;
		private const int sampleRate = 44100;
		private const int capacity = sampleRate * 10;
		private int position;
		private long sample;


		public Delay(Source sound, Source time)
		{
			this.time = time;
			this.sound = sound;
			
			buffer = new float[capacity];
		}

		public override Sample Play(IContext context)
		{
			var soundValue = sound.Play(context).Value;

			var delta = context.Sample - sample;
			sample = context.Sample;

			position += (int)delta;

			while (position >= capacity)
				position -= capacity;

			var writePosition = GetWritePosition(context);

			buffer[writePosition] = buffer[writePosition] / 2 + soundValue;

			return new Sample { Value = buffer[position] / 2 + soundValue };
		}

		public int GetWritePosition(IContext context)
		{
			var timeValue = (int)(time.Play(context).Value * sampleRate);

			if (timeValue <= 0)
				return position;
			if (timeValue >= capacity)
				timeValue = capacity - 2;

			var writePosition = position + timeValue;

			while (writePosition >= capacity)
				writePosition -= capacity;

			return writePosition;
		}
	}
}
