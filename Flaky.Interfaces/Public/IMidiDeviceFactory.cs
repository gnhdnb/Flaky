using System;
using System.Collections.Generic;
using System.Text;

namespace Flaky
{
	public interface IMidiDevice
	{
		float ReadLatestState(int controlNumber);
	}

	public interface IMidiDeviceFactory : IDisposable
	{
		IMidiDevice Create(string device);
	}
}
