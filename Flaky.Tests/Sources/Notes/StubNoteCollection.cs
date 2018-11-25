namespace Flaky.Tests
{
	internal class StubNoteCollection : INoteCollection
	{
		public bool Initialized { get; set; }
		public Note NextNote { get; set; }

		public void Dispose()
		{
			
		}

		public Note GetNextNote()
		{
			return NextNote;
		}

		public void Initialize(IFlakyContext context)
		{
			Initialized = true;
		}

		public void Reset()
		{
			
		}

		public void Update(IContext context)
		{
			
		}
	}
}
