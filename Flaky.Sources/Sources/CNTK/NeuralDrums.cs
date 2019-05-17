using CNTK;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Flaky
{
    public class NeuralDrums : Source
    {
		public override void Dispose()
		{
			
		}

		public void Load()
		{
			var model = Function.Load(
				@"D:\SampleSpace\net.onnx",
				DeviceDescriptor.CPUDevice,
				ModelFormat.ONNX);
		}

		protected override void Initialize(IContext context)
		{
			Load();
		}

		protected override Vector2 NextSample(IContext context)
		{
			return Vector2.Zero;
		}
	}
}
