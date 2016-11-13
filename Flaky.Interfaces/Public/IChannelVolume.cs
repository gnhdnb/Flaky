using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public interface IChannelVolume
	{
		int Channel { get; }
		float Value { get; }
	}
}
