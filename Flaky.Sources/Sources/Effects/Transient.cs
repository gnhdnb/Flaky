using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Transient : Source, IPipingSource
	{
		private Source source;
		private Source pitch;
		private Source sensitivity;
		private Source trigger;
		private State state;
		

		private class State
		{
			private double[] ampBuffer = new double[441];
			private int ampPosition = 0;
			private double lastAmp = 0;
			private bool triggered = false;
			private const int crossfadeInterval = 441;
			private long ticksFromLatestReset = 0;

			private Buffer primary = new Buffer();
			private Buffer secondary = new Buffer();

			private float currentCrossfade = 1;

			public State()
			{
				for(int i = 0; i < 441; i++)
				{
					ampBuffer[i] = -1;
				}
			}

			public void Reset()
			{
				var temp = primary;
				primary = secondary;
				secondary = temp;
				primary.Reset();
				currentCrossfade = 0;
				ticksFromLatestReset = 0;
			}

			public void WriteSample(Vector2 sample, float sensitivity, float trigger)
			{
				primary.MoveWriter();
				secondary.MoveWriter();

				if (sensitivity > 1)
					sensitivity = 1;

				var currentAmp = Math.Abs(sample.X);

				if (currentAmp > lastAmp)
				{
					lastAmp = currentAmp;
				}

				bool reset = false;

				if (trigger <= 0)
					triggered = false;

				if (!triggered && trigger > 0)
				{
					triggered = true;
					reset = true;
				}

				if (ampBuffer[ampPosition] >= 0
					&& lastAmp > ampBuffer[ampPosition] * (2 - sensitivity)
					&& ticksFromLatestReset > crossfadeInterval)
				{
					reset = true;
				}

				ampBuffer[ampPosition] = lastAmp;

				lastAmp = lastAmp - 0.01 * lastAmp;

				if (primary.ShouldReset(crossfadeInterval) || reset)
					Reset();

				primary.Write(sample);
				secondary.Write(sample);
				ticksFromLatestReset++;
			}

			public Vector2 ReadSample(float pitch)
			{
				primary.MoveReader(pitch);
				secondary.MoveReader(pitch);

				Crossfade();

				return primary.Read() * currentCrossfade + secondary.Read() * (1 - currentCrossfade);
			}

			private void Crossfade()
			{
				if(currentCrossfade < 1)
				{
					currentCrossfade += 1 / (float)crossfadeInterval;
				}

				if (currentCrossfade > 1)
					currentCrossfade = 1;
			}
		}

		internal Transient(Source pitch, string id) : this(pitch, 0.7, id) { }

		internal Transient(NoteSource pitch, string id) : this(pitch * (1 / Note.BaselineFrequency), 0.7, id) { }

		internal Transient(Source pitch, Source sensitivity, string id) : base(id)
		{
			this.pitch = pitch;
			this.sensitivity = sensitivity;
		}

		internal Transient(NoteSource pitch, Source sensitivity, string id)
			: this(pitch * (1 / Note.BaselineFrequency), sensitivity, id) { }

		internal Transient(NoteSource pitch, Source sensitivity, Source trigger, string id) 
			: this(pitch * (1 / Note.BaselineFrequency), sensitivity, trigger, id) { }

		internal Transient(Source pitch, Source sensitivity, Source trigger, string id) : base(id)
		{
			this.pitch = pitch;
			this.sensitivity = sensitivity;
			this.trigger = trigger;
		}

		public override void Dispose()
		{
			Dispose(source, pitch);
		}

		public override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			Initialize(context, trigger, source, pitch);
		}

		protected override Vector2 NextSample(IContext context)
		{
			var sensitivityValue = sensitivity.Play(context);
			var sourceValue = source.Play(context);
			var pitchValue = pitch.Play(context);

			float triggerValue = 0;

			if (trigger != null)
				triggerValue = trigger.Play(context).X;

			state.WriteSample(sourceValue, Math.Abs(sensitivityValue.X), triggerValue);

			return state.ReadSample(pitchValue.X);
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}

		private class Buffer
		{
			private const int bufferSize = 441000;
			private Vector2[] Samples = new Vector2[bufferSize];
			private float readerPosition = 0;
			private int writerPosition = 0;
			private int direction = 1;
			private bool writerOffline = false;

			public void Reset()
			{
				readerPosition = 0;
				writerPosition = 0;
				writerOffline = false;
			}

			public bool ShouldReset(int crossfadeInterval)
			{
				return writerPosition >= bufferSize - 1 - crossfadeInterval;
			}

			public void Write(Vector2 sample)
			{
				if(!writerOffline)
					Samples[writerPosition] = sample;

				if (writerPosition >= bufferSize - 1)
				{
					writerOffline = true;
				}
			}

			public void MoveWriter()
			{
				if (writerPosition < bufferSize - 1)
					writerPosition++;
			}

			public void MoveReader(float pitch)
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
			}

			public Vector2 Read()
			{
				var previousPosition = Math.Floor(readerPosition);
				var nextPosition = Math.Ceiling(readerPosition);

				var delta = readerPosition - previousPosition;

				return
					Samples[(int)previousPosition] * (1 - (float)delta)
					 + Samples[(int)nextPosition] * (float)delta;
			}
		}
	}
}
