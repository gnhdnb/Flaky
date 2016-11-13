using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apc40Controller
{
	public class Controller : IDisposable
	{
		private MidiIn input;
		private MidiOut output;
		private readonly List<NoteHandler> noteHandlers = new List<NoteHandler>();
		private readonly List<CcHandler> ccHandlers = new List<CcHandler>();

		public EventHandler<MatrixCoordinates> OnMatrixButtonPress;
		public EventHandler<int> OnChannelSelect;
		public EventHandler<ChannelVolume> OnMixerChange;

		public Controller()
		{
			int? deviceId = null;

			for (int i = 0; i < MidiIn.NumberOfDevices; i++)
				if (MidiIn.DeviceInfo(i).ProductName == "Akai APC40")
					deviceId = i;

			if (deviceId == null)
				throw new InvalidOperationException();

			RegisterHandlers();

			input = new MidiIn(deviceId.Value);

			input.MessageReceived += (o, e) =>
			{
				ProccessApcEvent(e);
			};

			input.Start();

			output = new MidiOut(deviceId.Value);
			var sysex = CreateInitEvent(deviceId.Value);
			output.SendBuffer(sysex);
		}

		private void RegisterHandlers()
		{
			RegisterNoteHandler(
				e => e.Channel >= 1 
				&& e.Channel <= 8 
				&& e.NoteNumber >= 53 
				&& e.NoteNumber <= 57 
				&& e.CommandCode == MidiCommandCode.NoteOn, 
				e =>
				{
					if (OnMatrixButtonPress != null)
						OnMatrixButtonPress(this, new MatrixCoordinates { Channel = e.Channel - 1, Scene = e.NoteNumber - 53 });
				});

			RegisterNoteHandler(
				e => e.Channel >= 1
				&& e.Channel <= 8
				&& e.NoteNumber == 51
				&& e.CommandCode == MidiCommandCode.NoteOn,
				e => 
				{
					if(OnChannelSelect != null)
						OnChannelSelect(this, e.Channel - 1);
				});

			RegisterCcHandler(
				e => e.Channel >= 1
				&& e.Channel <= 8
				&& ((byte)e.Controller) == 7,
				e=>
				{
					if (OnMixerChange != null)
						OnMixerChange(this, new ChannelVolume { Channel = e.Channel - 1, Value = e.ControllerValue });
				});
		}

		private void RegisterNoteHandler(Func<NoteEvent, bool> canHandle, Action<NoteEvent> handle)
		{
			noteHandlers.Add(new NoteHandler(canHandle, handle));
		}

		private void RegisterCcHandler(Func<ControlChangeEvent, bool> canHandle, Action<ControlChangeEvent> handle)
		{
			ccHandlers.Add(new CcHandler(canHandle, handle));
		}

		private void ProcessNote(NoteEvent note)
		{
			noteHandlers.ForEach(c => c.HandleEvent(note));
		}

		private void ProcessCc(ControlChangeEvent cc)
		{
			ccHandlers.ForEach(c => c.HandleEvent(cc));
		}

		private void SetMatrixLed(int row, int column, int mode)
		{
			var e = new NoteEvent(DateTime.Now.Ticks, column + 1, MidiCommandCode.NoteOn, 53 + row, mode);

			output.Send(e.GetAsShortMessage());
		}

		private void ProccessApcEvent(MidiInMessageEventArgs e)
		{
			if (e.MidiEvent is NoteEvent)
			{
				var note = e.MidiEvent as NoteEvent;

				ProcessNote(note);
			}
			else if (e.MidiEvent is ControlChangeEvent)
			{
				var cc = e.MidiEvent as ControlChangeEvent;

				ProcessCc(cc);
			}
		}

		private byte[] CreateInitEvent(int deviceId)
		{
			return new byte[]
			{
				0xF0, // MIDI System exclusive message start
				0x47, // Manufacturers ID Byte
				(byte)deviceId, // System Exclusive Device ID
				0x73, // Product model ID
				0x60, // Message type identifier
				0x00, // Number of data bytes to follow (most significant) 
				0x04, // Number of data bytes to follow (least significant)
				0x42, // Application/Configuration identifier
				0x01, // PC application Software version major
				0x01, // PC application Software version minor
				0x01, // PC Application Software bug-fix level
				0xF7, // MIDI System exclusive message terminator
			};
		}

		public void Dispose()
		{
			input.Dispose();
			output.Dispose();
		}
	}
}
