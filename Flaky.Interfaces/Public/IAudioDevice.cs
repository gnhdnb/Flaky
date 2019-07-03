using System;
using System.Collections.Generic;
using System.Text;

namespace Flaky
{
	public interface IAudioDevice : IDisposable
	{
		void Init(IBufferedSource source, string recordFilePath = null);
		void Play();
		void Stop();
	}
}
