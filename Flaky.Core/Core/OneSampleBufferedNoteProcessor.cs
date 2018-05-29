namespace Flaky
{
	internal class OneSampleBufferedNoteProcessor : IExternalNoteSourceProcessor
	{
		private readonly IFlakyNoteSource flakySource;

		private PlayingNote latestNote;
		private long latestNoteIndex = -1;

		public OneSampleBufferedNoteProcessor(IFlakyNoteSource flakySource)
		{
			this.flakySource = flakySource;
		}

		public PlayingNote GetNote(IContext context)
		{
			if (context.Sample == latestNoteIndex)
				return latestNote;

			var result = flakySource.GetNoteInCurrentThread(context);

			latestNote = result;
			latestNoteIndex = context.Sample;

			return result;
		}
	}
}