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

		public Host(int channelsCount, string outputWaveFilePath = null)
		{
			var configuration = new Configuration();

			configuration.Register<IWaveReaderFactory>(new WaveReaderFactory());
			configuration.Register<IWaveWriterFactory>(new WaveWriterFactory());

			Compiler = new Compiler(new[] {
				typeof(Source).Assembly,
				typeof(Mixer).Assembly
			});

			Device = new WaveOut();
			Mixer = new Mixer(channelsCount, 44100, 13230, 120, configuration);
			Adapter = new WaveAdapter(Mixer);

			if (outputWaveFilePath != null)
			{
				Recorder = new WaveRecorder(Adapter, outputWaveFilePath);
				Device.Init(Recorder);
			}
			else
			{
				Device.Init(Adapter);
			}
		}

		public string[] Recompile(int channel, string code)
		{
			var result = Compiler.Compile(code);

			if (!result.Success)
				return result.Messages;

			Mixer.ChangePlayer(channel, result.Player);
			return new string[0];
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
			Recorder?.Dispose();
			Mixer.Dispose();
		}
	}
}
