using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace Flaky.Tests
{
	[TestClass]
	public class ValueAsNoteTests
	{
		[TestMethod]
		public void ValueAsNoteSequencing()
		{
			var valueAsNote = new ValueAsNote(2, "id1");
			var stubSource = new StubSource();
			var context = new StubContext();

			valueAsNote.SetMainSource(stubSource);

			valueAsNote.Initialize(null, context);

			Assert.IsTrue(stubSource.Initialized);

			stubSource.NextSample = Vector2.One * 0;
			Assert.AreEqual(0, valueAsNote.GetNote(context).Note.Number);
			context.Beat++;
			stubSource.NextSample = Vector2.One * 0.99f;
			Assert.AreEqual(2, valueAsNote.GetNote(context).Note.Number);
			context.Beat++;
			stubSource.NextSample = Vector2.One * -1.99f;
			Assert.AreEqual(-4, valueAsNote.GetNote(context).Note.Number);
		}
	}
}
