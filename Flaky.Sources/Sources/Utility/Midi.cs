using System.Numerics;

namespace Flaky
{
	public class Midi : Source
	{
		public Midi(string deviceName, int controlNumber, string id) : base(id) 
		{ 
			this.deviceName = deviceName;
			this.controlNumber = controlNumber;
		}

		private readonly string deviceName;
		private readonly int controlNumber;
		private State state;

		internal class State
		{
			public float Value = 0;
			public IMidiDevice Device;
		}

		protected override Vector2 NextSample(IContext context)
		{	
			var output = state.Device.ReadLatestState(controlNumber) / 127f;
			
			var delta = output - state.Value;

			state.Value += delta / 2200;

			return new Vector2(state.Value, state.Value);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			state.Device = Get<IMidiDeviceFactory>(context).Create(deviceName);
		}

		public override void Dispose()
		{
			// do nothing
		}
	}
}
