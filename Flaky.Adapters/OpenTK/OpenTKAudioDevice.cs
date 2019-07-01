using System;
using System.Collections.Generic;
using System.Text;

namespace Flaky
{
	public class OpenTKAudioDevice : IAudioDevice
	{
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public void Init(IBufferedSource source, string recordFilePath = null)
		{
			throw new NotImplementedException();
		}

		public void Play()
		{
			throw new NotImplementedException();
		}

		public void Stop()
		{
			throw new NotImplementedException();
		}
	}
}
