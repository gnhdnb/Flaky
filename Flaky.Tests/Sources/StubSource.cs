using System.Numerics;

namespace Flaky.Tests
{
	public class StubSource : ISource
	{
		public bool Initialized { get; set; }
		public bool Disposed { get; set; }

		public Vector2 NextSample { get; set; }

		public void Dispose()
		{
			Disposed = true;
		}

		public void Initialize(ISource parent, IContext context)
		{
			Initialized = true;
		}

		public Vector2 Play(IContext context)
		{
			return NextSample;
		}
	}
}
