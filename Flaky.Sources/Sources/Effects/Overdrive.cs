using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class Overdrive : Source, IPipingSource
	{
		private Source source;
		private Source overdrive;
		private Source lpnoise;
		private Source hpnoise;

		internal Overdrive(Source overdrive, string id)
		{
			this.overdrive = overdrive;
			this.lpnoise = new OnePoleLPFilter(new Noise(), 0.001f, $"{id}_lpnoise") * 100f;
			this.hpnoise = new OnePoleHPFilter(new Noise(), 1, $"{id}_hpnoise") * 0.3f;
		}

		protected override void Initialize(IContext context)
		{
			Initialize(context, source, overdrive, lpnoise, hpnoise);
		}

		public override void Dispose()
		{
			Dispose(source, overdrive, lpnoise, hpnoise);
		}

		protected override Sample NextSample(IContext context)
		{
			var sample = source.Play(context).Value;
			var overdriveValue =
				overdrive.Play(context).Value 
				+ 1
				+ lpnoise.Play(context).Value * Math.Abs(sample) * Math.Abs(sample)
				+ hpnoise.Play(context).Value * Math.Abs(sample) * Math.Abs(sample) * Math.Abs(sample) * Math.Abs(sample);

			if (overdriveValue < 1)
				overdriveValue = 1;

			var result = OD(sample * overdriveValue) / overdriveValue;

			return new Sample
			{
				Left = result,
				Right = result
			};
		}

		private float OD(float value)
		{
			return 2.5f * Math.Sign(value) * (1 - (float)Math.Pow(Math.E, -Math.Abs(value)));
		}

		void IPipingSource<Source>.SetMainSource(Source mainSource)
		{
			this.source = mainSource;
		}
	}
}
