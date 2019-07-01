using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flaky
{
	public class NAudioDevice : IAudioDevice
	{
		private bool initialized = false;
		private WasapiOut Device { get; set; }
		private WaveAdapter Adapter { get; set; }
		private WaveRecorder Recorder { get; set; }

		public void Dispose()
		{
			Device.Dispose();
			Recorder?.Dispose();
		}

		public void Init(IBufferedSource source, string recordFilePath = null)
		{
			if (initialized)
				throw new InvalidOperationException("Already initialized");

			initialized = true;

			Device = new WasapiOut();
			Adapter = new WaveAdapter(source);

			if(recordFilePath != null)
			{
				Recorder = new WaveRecorder(Adapter, recordFilePath);
				Device.Init(Recorder);
			}
			else
			{
				Device.Init(Adapter);
			}
		}

		public void Play()
		{
			Device.Play();
		}

		public void Stop()
		{
			Device.Stop();
		}
	}
}
