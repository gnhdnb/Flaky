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
		private IMixerController MixerController { get; }

		public Host(int channelsCount, string outputWaveFilePath = null, IMixerController mixerController = null)
		{
			var configuration = new Configuration();

			configuration.Register<IWaveReaderFactory>(new WaveReaderFactory());
			configuration.Register<IWaveWriterFactory>(new WaveWriterFactory());

			MixerController = mixerController;

			if (MixerController != null)
				MixerController.OnMixerChange += OnMixerChange;

			Compiler = new Compiler(typeof(Source).Assembly);
			Device = new WaveOut();
			Mixer = new Mixer(channelsCount, configuration);
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

		public (string[], SourceTreeNode) Recompile(int channel, string code)
		{
			var result = Compiler.Compile(code);

			if (!result.Success)
				return (result.Messages, null);

			var root = Mixer.ChangePlayer(channel, result.Player);
			return (new string[0], root);
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

		private void OnMixerChange(object sender, IChannelVolume volume)
		{
			Mixer.SetVolume(volume.Channel, volume.Value);
		}
	}
}
