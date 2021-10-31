using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace Flaky
{
	public abstract class ChromaticFieldSource : Source
	{

		public ChromaticFieldSource(string id) : base(id) { }

		public ChromaticFieldSource() : base() { }

		public abstract float[] GetField(IContext context);

		protected sealed override Vector2 NextSample(IContext context)
		{
			return new Vector2(0, 0);
		}
	}

	public class WebFieldSource : ChromaticFieldSource
	{
		private bool visualize = false;

		private ISource s1;
		private ISource s2;
		private ISource s3;
		private ISource s4;
		private ISource s5;

		private class State : IDisposable
		{
			private string url;
			private Thread worker;
			private bool visualize;

			private float[,] currentBar = new float[60, 60];
			private float[,] nextBar = new float[60, 60];
			private float[,] currentInputFeatures = new float[60, 5];
			private float[,] nextInputFeatures = new float[60, 5];
			private volatile bool nextChunkNeeded = false;
			private ManualResetEvent nextChunk = new ManualResetEvent(false);

			private bool initialized = false;
			private bool disposing = false;
			private IWebClient webClient;
			private IErrorOutput errorOutput;

			private float[] currentField = new float[60];
			private long currentSample = 0;
			private long readerStartSample = 0;
			private int readerPosition = 0;

			public void Initialize(
				IFlakyContext context,
				string url,
				bool visualize)
			{
				this.url = url;
				this.visualize = visualize;

				if (initialized)
					return;

				initialized = true;

				webClient = context.Get<IWebClient>();
				errorOutput = context.Get<IErrorOutput>();

				worker = new Thread(DownloadLoop);

				worker.Start();
			}

			private void DownloadLoop()
			{
				while (!disposing)
				{
					if (nextChunkNeeded)
					{
						nextChunk.Reset();
						nextChunkNeeded = false;

						try
						{
							using (var chunk = webClient.Post(url, currentInputFeatures))
							using (var reader = new StreamReader(chunk))
							{
								var data = reader.ReadToEnd().Split(',');

								for(int i = 0; i < 60; i++)
									for(int j = 0; j < 60; j++)
										nextBar[i, j] = float.Parse(data[i * 60 + j], CultureInfo.InvariantCulture);
							}
						}
						catch (Exception ex)
						{
							errorOutput.WriteLine(ex.ToString());
						}
					}

					nextChunk.WaitOne(100);
				}
			}

			private void NextChunk()
			{
				if (!nextChunkNeeded)
				{
					var tempBar = currentBar;
					currentBar = nextBar;
					nextBar = tempBar;

					var tempFeatures = currentInputFeatures;
					currentInputFeatures = nextInputFeatures;
					nextInputFeatures = tempFeatures;

					nextChunkNeeded = true;
					nextChunk.Set();
				}
			}

			public void Dispose()
			{
				disposing = true;
				worker.Join();
			}

			public float[] GetField(IContext context,
				float s1Value,
				float s2Value,
				float s3Value,
				float s4Value,
				float s5Value)
			{
				if (context.Sample <= currentSample)
					return currentField;

				currentSample = context.Sample;

				if (context.Sample > readerStartSample + 221 * 4)
				{
					readerStartSample = context.Sample;

					readerPosition++;

					if (readerPosition >= 60)
					{
						NextChunk();
						readerPosition = 0;
					}

					nextInputFeatures[readerPosition, 0] = s1Value;
					nextInputFeatures[readerPosition, 1] = s2Value;
					nextInputFeatures[readerPosition, 2] = s3Value;
					nextInputFeatures[readerPosition, 3] = s4Value;
					nextInputFeatures[readerPosition, 4] = s5Value;

					for (int i = 0; i < 60; i++)
					{
						currentField[i] = currentBar[readerPosition, i];
					}
				}

				return currentField;
			}
		}

		private State state;
		private string url;

		public WebFieldSource(string url, bool visualize,
			ISource s1,
			ISource s2,
			ISource s3,
			ISource s4,
			ISource s5,
			string id) : base(id)
		{
			this.url = url;
			this.visualize = visualize;
			this.s1 = s1;
			this.s2 = s2;
			this.s3 = s3;
			this.s4 = s4;
			this.s5 = s5;
		}

		public override void Dispose()
		{
			Dispose(s1, s2, s3, s4, s5);
		}

		public override float[] GetField(IContext context)
		{
			var s1Value = s1.Play(context).X;
			var s2Value = s2.Play(context).X;
			var s3Value = s3.Play(context).X;
			var s4Value = s4.Play(context).X;
			var s5Value = s5.Play(context).X;

			return state.GetField(
				context, s1Value, s2Value, s3Value, s4Value, s5Value);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			Initialize(context, s1, s2, s3, s4, s5);
			state.Initialize((IFlakyContext)context, url, visualize);
		}
	}

	public class TestFieldSource : ChromaticFieldSource
	{
		private float[] testField = new float[60];

		public TestFieldSource()
		{
			for (int i = 0; i < 60; i++)
			{
				testField[i] = i / 60f;
			}
		}

		public override void Dispose()
		{
			
		}

		public override float[] GetField(IContext context)
		{
			return testField;
		}

		protected override void Initialize(IContext context)
		{
			
		}
	}
}
