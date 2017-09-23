using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Transient : Source
	{
		private Source source;
		private Source pitch;
		private Source sensitivity;
		private State state;
		

		private class State
		{
			private const int bufferSize = 44100;
			public Sample[] Buffer = new Sample[bufferSize];
			private double[] ampBuffer = new double[441];
			private int ampPosition = 0;
			private float readerPosition = 0;
			private int writerPosition = 0;
			private int direction = 1;
			private double lastAmp = 0;

			public State()
			{
				for(int i = 0; i < 441; i++)
				{
					ampBuffer[i] = -1;
				}
			}

			public void Reset()
			{
				readerPosition = 0;
				writerPosition = 0;
			}

			public void WriteSample(Sample sample, float sensitivity)
			{
				writerPosition++;

				if (sensitivity > 1)
					sensitivity = 1;

				var currentAmp = Math.Abs(sample.Value);

				if (currentAmp > lastAmp)
				{
					lastAmp = currentAmp;
				}

				bool reset = false;

				if (ampBuffer[ampPosition] >= 0
					&& lastAmp > ampBuffer[ampPosition] * (2 - sensitivity))
					reset = true;

				ampBuffer[ampPosition] = lastAmp;

				lastAmp = lastAmp - 0.01 * lastAmp;

				if (writerPosition >= bufferSize || reset)
					Reset();

				Buffer[writerPosition] = sample;
			}

			public Sample ReadSample(float pitch)
			{
				readerPosition += direction * pitch;

				if (readerPosition < 0)
				{
					readerPosition = -readerPosition;
					direction = -direction;
				}

				if (readerPosition > writerPosition)
				{
					readerPosition = writerPosition - (readerPosition - writerPosition);
					direction = -direction;
				}

				if (readerPosition < 0)
				{
					readerPosition = 0;
					direction = 1;
				}

				var previousPosition = Math.Floor(readerPosition);
				var nextPosition = Math.Ceiling(readerPosition);

				var delta = readerPosition - previousPosition;

				return
					Buffer[(int)previousPosition] * (1 - (float)delta)
					 + Buffer[(int)nextPosition] * (float)delta;
			}
		}

		public Transient(Source source, Source pitch, string id) : this(source, pitch, 0.7, id) { }

		public Transient(Source source, Source pitch, Source sensitivity, string id) : base(id)
		{
			this.source = source;
			this.pitch = pitch;
			this.sensitivity = sensitivity;
		}

		public override void Dispose()
		{
			Dispose(source, pitch);
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			Initialize(context, source, pitch);
		}

		public override Sample Play(IContext context)
		{
			var sensitivityValue = sensitivity.Play(context);
			var sourceValue = source.Play(context);
			var pitchValue = pitch.Play(context);

			state.WriteSample(sourceValue, Math.Abs(sensitivityValue.Value));

			return state.ReadSample(pitchValue.Value);
		}
	}
}
