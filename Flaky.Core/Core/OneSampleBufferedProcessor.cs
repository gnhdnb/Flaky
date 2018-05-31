namespace Flaky
{
	internal class OneSampleBufferedProcessor : IExternalSourceProcessor
	{
		private readonly IFlakySource flakySource;

		private Sample latestSample;
		private long latestSampleIndex = -1;

		public OneSampleBufferedProcessor(IFlakySource flakySource)
		{
			this.flakySource = flakySource;
		}

		public Sample Play(IContext context)
		{
			if (context.Sample == latestSampleIndex)
				return latestSample;

			var result = flakySource.PlayInCurrentThread(context);

			latestSample = result;
			latestSampleIndex = context.Sample;

			return result;
		}
	}
}