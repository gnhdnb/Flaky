using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Flaky.Tests
{
	[TestClass]
	public class VariableLengthSequenceTests
	{
		[TestMethod]
		public void Sequencing()
		{
			var noteCollection = new StubNoteCollection();
			var lengthSource = new StubSource();
			var sequence = new VariableLengthSequence(noteCollection, lengthSource, false, "id1");
			var context = new StubContext();

			sequence.Initialize(context);

			var notes = new[] {
				new Note(5),
				new Note(6)
			};

			lengthSource.NextSample = new Vector2 { X = 1, Y = 1 };

			context.Beat = 0;
			noteCollection.NextNote = notes[0];
			var playingNote = sequence.GetNote(context);
			Assert.AreEqual(notes[0], playingNote.Note);

			noteCollection.NextNote = notes[1];

			context.Sample = 22100;
			playingNote = sequence.GetNote(context);
			Assert.AreNotEqual(notes[1], playingNote.Note);
			Assert.AreEqual(0, playingNote.StartSample);

			context.Beat = 44100;
			playingNote = sequence.GetNote(context);
			Assert.AreEqual(notes[1], playingNote.Note);
			Assert.AreEqual(context.Sample, playingNote.StartSample);
		}
	}
}
