using NAudio.Midi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal class Host : IDisposable
	{
		private Compiler Compiler { get; }
		private WaveOut Device { get; }
		private Mixer Mixer { get; }
		private WaveAdapter Adapter { get; }
		private WaveRecorder Recorder { get; }

		internal Host(int channelsCount, string outputWaveFilePath)
		{
			Compiler = new Compiler();
			Device = new WaveOut();
			Mixer = new Mixer(channelsCount);
			Adapter = new WaveAdapter(Mixer);
			Recorder = new WaveRecorder(Adapter, outputWaveFilePath);
			Device.Init(Recorder);
		}

		internal bool Recompile(int channel, string code)
		{
			var result = Compiler.Compile(code);

			if (!result.Success)
				return false;

			Mixer.ChangePlayer(channel, result.Player);
			return true;
		}

		internal void Play()
		{
			Device.Play();
		}

		internal void Stop()
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
