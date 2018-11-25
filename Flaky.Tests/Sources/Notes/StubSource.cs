namespace Flaky.Tests
{
	public class StubSource : ISource
	{
		public bool Initialized { get; set; }
		public bool Disposed { get; set; }

		public Sample NextSample { get; set; }

		public void Dispose()
		{
			Disposed = true;
		}

		public void Initialize(IContext context)
		{
			Initialized = true;
		}

		public Sample Play(IContext context)
		{
			return NextSample;
		}
	}
}
