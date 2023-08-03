using System;
using System.Linq;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace Flaky 
{
	public class DryWetMidiDevice : IMidiDevice
	{
		private readonly InputDevice inputDevice;
		private readonly ushort[] inputs = new ushort[128];

		internal DryWetMidiDevice(InputDevice inputDevice)
		{
			this.inputDevice = inputDevice;
			this.inputDevice.EventReceived += OnEventReceived;
			this.inputDevice.StartEventsListening();
		}

		public void Dispose()
		{
			inputDevice.Dispose();
		}

		public float ReadLatestState(int controlNumber)
		{
			if(controlNumber < 0 || controlNumber > 127)
				return 0;

			return inputs[controlNumber];
		}

		private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
		{
			if (e.Event.EventType == MidiEventType.ControlChange)
			{
				var cc = (ControlChangeEvent)e.Event;

				inputs[cc.ControlNumber] = cc.ControlValue;

				Console.WriteLine($"{cc.ControlNumber}, {cc.ControlValue}");
			}
		}
	}

	public class DryWetMidiDeviceFactory : IMidiDeviceFactory
	{
		private readonly Dictionary<string, DryWetMidiDevice> devices 
			= new Dictionary<string, DryWetMidiDevice>();

		public IMidiDevice Create(string device)
		{
			if(devices.ContainsKey(device))
				return devices[device];

			try {
				devices.Add(device, new DryWetMidiDevice(InputDevice.GetByName(device)));
			} catch (ArgumentException ex)
			{
				throw new ArgumentException($"{device} not found. Available devices are: " + 
					string.Concat(InputDevice.GetAll().Select(d => $"'{d.Name}' ")), ex
				);
			}

			return devices[device];
		}

		public void Dispose()
		{
			foreach(var device in devices.Values)
				device.Dispose();
		}
	}
}