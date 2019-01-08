using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flaky.Tests
{
	[TestClass]
	public class FixLengthSequenceTests
	{
		[TestMethod]
		public void Initialization()
		{
			var noteCollection = new StubNoteCollection();
			var sequence = new FixLengthSequence(noteCollection, 4, false, "id1");
			var context = new StubContext();

			sequence.Initialize(null, context);

			Assert.IsTrue(noteCollection.Initialized);
		}

		[TestMethod]
		public void QuantizedSequenceStart()
		{
			var noteCollection = new StubNoteCollection();
			var sequence = new FixLengthSequence(noteCollection, 4, false, "id1");
			var context = new StubContext();

			sequence.Initialize(null, context);

			context.Beat = 3;

			noteCollection.NextNote = new Note(5);

			var playingNote = sequence.GetNote(context);

			Assert.AreEqual(null, playingNote.Note);

			context.Beat = 4;

			playingNote = sequence.GetNote(context);

			Assert.AreEqual(noteCollection.NextNote, playingNote.Note);
		}

		[TestMethod]
		public void Sequencing()
		{
			var noteCollection = new StubNoteCollection();
			var sequence = new FixLengthSequence(noteCollection, 4, false, "id1");
			var context = new StubContext();

			sequence.Initialize(null, context);

			var notes = new[] {
				new Note(5),
				new Note(6)
			};

			context.Beat = 0;
			noteCollection.NextNote = notes[0];
			var playingNote = sequence.GetNote(context);
			Assert.AreEqual(notes[0], playingNote.Note);

			noteCollection.NextNote = notes[1];

			context.Beat = 1;
			playingNote = sequence.GetNote(context);
			Assert.AreNotEqual(notes[1], playingNote.Note);
			Assert.AreEqual(0, playingNote.StartSample);

			context.Beat = 4;
			playingNote = sequence.GetNote(context);
			Assert.AreEqual(notes[1], playingNote.Note);
			Assert.AreEqual(context.Sample, playingNote.StartSample);
		}

		[TestMethod]
		public void SequenceReset()
		{
			var noteCollection = new StubNoteCollection();
			var resetSequence = new FixLengthSequence(
				new SequentialNoteCollection("0", "resetSource"),
				8,
				false,
				"resetSource");

			var sequence = new FixLengthSequence(
				new SequentialNoteCollection("0123", "testSource"),
				16,
				false,
				"testSource",
				resetSequence);

			var context = new StubContext();

			sequence.Initialize(null, context);

			context.Beat = 0;

			var playingNote = sequence.GetNote(context);

			Assert.AreEqual(0, playingNote.Note?.Number);

			context.Beat = 1;

			playingNote = sequence.GetNote(context);

			Assert.AreEqual(1, playingNote.Note?.Number);

			context.Beat = 2;

			playingNote = sequence.GetNote(context);

			Assert.AreEqual(0, playingNote.Note?.Number);

			context.Beat = 3;

			playingNote = sequence.GetNote(context);

			Assert.AreEqual(1, playingNote.Note?.Number);
		}
	}
}
