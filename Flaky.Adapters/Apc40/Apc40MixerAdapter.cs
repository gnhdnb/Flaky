using Apc40Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Apc40MixerAdapter : IMixerController, IDisposable
	{
		private Controller controller;

		public Apc40MixerAdapter(Controller controller)
		{
			this.controller = controller;
			controller.OnMixerChange += MixerChange;
		}

		public EventHandler<IChannelVolume> OnMixerChange { get; set; }

		public void Dispose()
		{
			controller.Dispose();
		}

		private void MixerChange(object sender, Apc40Controller.ChannelVolume volume)
		{
			if(OnMixerChange != null)
				OnMixerChange(
					this, 
					new ChannelVolume
					{
						Channel = volume.Channel,
						Value = ((float)volume.Value / 128)
					});
		}
	}

	internal class ChannelVolume : IChannelVolume
	{
		public int Channel { get; set; }
		public float Value { get; set; }
	}
}
