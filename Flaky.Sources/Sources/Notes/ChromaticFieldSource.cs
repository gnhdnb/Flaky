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
		private List<WebTask> tasks;

		private class WebTask
		{
			public int Point1Batch { get; set; }
			public int Point1Roll { get; set; }
			public int Point2Batch { get; set; }
			public int Point2Roll { get; set; }
			public float Lerp { get; set; }
			public float Amp { get; set; }
			public float S1 { get; set; }
			public float S2 { get; set; }
			public float S3 { get; set; }
			public float S4 { get; set; }
			public float S5 { get; set; }
		}

		private class State : IDisposable
		{
			private string url;
			private Thread worker;

			private float[,] currentBar = new float[60, 60];
			private float[,] nextBar = new float[60, 60];
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

			private int currentTaskIndex = 0;
			private List<WebTask> tasks = new List<WebTask>();

			public void Initialize(
				IFlakyContext context,
				string url,
				List<WebTask> tasks)
			{
				this.url = url;
				this.tasks = tasks;

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

						WebTask task;

						if (tasks.Count > 0)
						{
							currentTaskIndex = currentTaskIndex % tasks.Count;
							task = tasks[currentTaskIndex];
							currentTaskIndex++;
						} else
						{
							currentTaskIndex = 0;

							task = new WebTask
							{
								Point1Batch = 1,
								Point1Roll = 0,
								Point2Batch = 1,
								Point2Roll = 0,
								Lerp = 0,
								Amp = 1
							};
						}

						try
						{
							var requestUri =
								$"{url}?p1b={task.Point1Batch.ToString(CultureInfo.InvariantCulture)}" +
								$"&p1r={task.Point1Roll.ToString(CultureInfo.InvariantCulture)}" +
								$"&p2b={task.Point2Batch.ToString(CultureInfo.InvariantCulture)}" +
								$"&p2r={task.Point2Roll.ToString(CultureInfo.InvariantCulture)}" +
								$"&lerp={task.Lerp.ToString(CultureInfo.InvariantCulture)}" +
								$"&amp={task.Amp.ToString(CultureInfo.InvariantCulture)}" +
								$"&s1={task.S1.ToString(CultureInfo.InvariantCulture)}" +
								$"&s2={task.S2.ToString(CultureInfo.InvariantCulture)}" +
								$"&s3={task.S3.ToString(CultureInfo.InvariantCulture)}" +
								$"&s4={task.S4.ToString(CultureInfo.InvariantCulture)}" +
								$"&s5={task.S5.ToString(CultureInfo.InvariantCulture)}";

							using (var chunk = webClient.Get(requestUri))
							using (var reader = new StreamReader(chunk))
							{
								var data = reader.ReadToEnd().Split(',');

								for(int i = 0; i < 60; i++)
									for(int j = 0; j < 60; j++)
										nextBar[j, i] = float.Parse(data[i * 60 + j], CultureInfo.InvariantCulture);
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
					currentBar = nextBar;
					nextChunkNeeded = true;
					nextChunk.Set();
				}
			}

			public void Dispose()
			{
				disposing = true;
				worker.Join();
			}

			public float[] GetField(IContext context)
			{
				if (context.Sample <= currentSample)
					return currentField;

				currentSample = context.Sample;

				if (context.Sample > readerStartSample + 2210)
				{
					readerStartSample = context.Sample;

					readerPosition++;

					if(readerPosition >= 60)
					{
						NextChunk();
						readerPosition = 0;
					}

					for(int i = 0; i < 60; i++)
					{
						currentField[i] = currentBar[readerPosition, i];
					}
				}

				return currentField;
			}
		}

		private State state;
		private string url;

		public WebFieldSource(string url, IEnumerable<(int, int, int, int, float, float, float[])> tasks, string id) : base(id)
		{
			this.url = url;
			this.tasks = tasks.Select(t => new WebTask
			{
				Point1Batch = t.Item1,
				Point1Roll = t.Item2,
				Point2Batch = t.Item3,
				Point2Roll = t.Item4,
				Lerp = t.Item5,
				Amp = t.Item6,
				S1 = t.Item7[0],
				S2 = t.Item7[1],
				S3 = t.Item7[2],
				S4 = t.Item7[3],
				S5 = t.Item7[4]
			}).ToList();
		}

		public override void Dispose()
		{
			
		}

		public override float[] GetField(IContext context)
		{
			return state.GetField(context);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			state.Initialize((IFlakyContext)context, url, tasks);
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
