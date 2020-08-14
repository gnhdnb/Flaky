using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;

namespace Flaky
{
	public class Tacotron : Source
	{
		private State state;
		private Source modulation;
		private string url;
		private string mode;

		public Tacotron(string url, string mode, Source modulation, string id) : base(id)
		{
			this.url = url;
			this.mode = mode;
			this.modulation = modulation;
		}

		private class State : IDisposable
		{
			private string url;
			private Thread worker;

			private int position = 0;
			private volatile float[] buffer1 = null;
			private volatile float[] buffer2 = new float[22050];
			private volatile bool nextChunkNeeded = false;
			private volatile float mod = 0;
			private volatile string mode = "";
			private ManualResetEvent nextChunk = new ManualResetEvent(false);

			private bool initialized = false;
			private bool disposing = false;
			private IAudioStreamReader vorbisReader;
			private IWebClient webClient;
			private IErrorOutput errorOutput;

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
							using (var chunk = webClient.Get(url + $"?mod={mod}&mode={mode}"))
							{
								//buffer2 = vorbisReader.ReadVorbis(chunk);
								buffer2 = vorbisReader.ReadWav(chunk);
							}
						} 
						catch(Exception ex)
						{
							errorOutput.WriteLine(ex.ToString());
						}
					}

					nextChunk.WaitOne(100);
				}
			}

			private void NextChunk(float mod)
			{
				if (!nextChunkNeeded)
				{
					buffer1 = buffer2;
					this.mod = mod;
					nextChunkNeeded = true;
					nextChunk.Set();
				}
			}

			public Vector2 Read(IContext context, Vector2 mod)
			{
				float value;

				if (buffer1 == null || position >= buffer1.Length * 2)
				{
					NextChunk(mod.X);

					position = 0;
				} 

				if (position % 2 == 0)
					value = buffer1[position / 2];
				else
					value = buffer1[position / 2] * 0.5f + buffer1[Math.Min(position / 2 + 1, buffer1.Length - 1)] * 0.5f;

				position++;
				return new Vector2(value, value);
			}

			public void Initialize(IFlakyContext context, string url, string mode)
			{
				this.url = url;
				this.mode = mode;

				if (initialized)
					return;

				initialized = true;

				vorbisReader = context.Get<IAudioStreamReader>();
				webClient = context.Get<IWebClient>();
				errorOutput = context.Get<IErrorOutput>();

				worker = new Thread(DownloadLoop);

				worker.Start();
			}

			public void Dispose()
			{
				disposing = true;
				worker.Join();
			}
		}

		public override void Dispose()
		{
			Dispose(modulation);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			state.Initialize((IFlakyContext)context, url, mode);

			Initialize(context, modulation);
		}

		protected override Vector2 NextSample(IContext context)
		{
			var mod = modulation?.Play(context) ?? Vector2.Zero;

			return state.Read(context, mod);
		}
	}
}
