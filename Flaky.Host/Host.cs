using NAudio.Midi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Host : IDisposable
	{
		private Compiler Compiler { get; }
		private WaveOut Device { get; }
		private Mixer Mixer { get; }
		private WaveAdapter Adapter { get; }
		private WaveRecorder Recorder { get; }

		public Host(int channelsCount, string outputWaveFilePath)
		{
			var configuration = new Configuration();

			configuration.Register<IWaveReaderFactory>(new WaveReaderFactory());

			Compiler = new Compiler(typeof(Source).Assembly);
			Device = new WaveOut();
			Mixer = new Mixer(channelsCount, configuration);
			Adapter = new WaveAdapter(Mixer);
			Recorder = new WaveRecorder(Adapter, outputWaveFilePath);
			Device.Init(Recorder);
		}

		public bool Recompile(int channel, string code)
		{
			var result = Compiler.Compile(code);

			if (!result.Success)
				return false;

			Mixer.ChangePlayer(channel, result.Player);
			return true;
		}

		public void Play()
		{
			Device.Play();
		}

		public void Stop()
		{
			Device.Stop();
		}

		public void Dispose()
		{
			Device.Dispose();
			Recorder.Dispose();
			Mixer.Dispose();
		}
	}
}
