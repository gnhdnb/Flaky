using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal class Host
	{
		private string Code { get; set; }
		private Compiler Compiler { get; }
		private WaveOut Device { get; }
		private WaveAdapter Adapter { get; }

		internal Host()
		{
			Compiler = new Compiler();
			Device = new WaveOut();
			Adapter = new WaveAdapter();
			Device.Init(Adapter);
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
	}
}
