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
		private string Code { get; set; }
		private Compiler Compiler { get; }
		private WaveOut Device { get; }
		private WaveAdapter Adapter { get; }
		private WaveRecorder Recorder { get; }

		internal Host()
		{
			Compiler = new Compiler();
			Device = new WaveOut();
			Adapter = new WaveAdapter();
			Recorder = new WaveRecorder(Adapter, @"D:\temp\flaky.wav");
			Device.Init(Recorder);
		}

		internal bool Recompile(string code)
		{
			Code = code;
			var result = Compiler.Compile(Code);

			if (!result.Success)
				return false;

			Adapter.ChangePlayer(result.Player);
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
		}
	}
}
