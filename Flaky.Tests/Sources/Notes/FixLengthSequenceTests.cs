using System;
using System.Collections.Generic;
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

			sequence.Initialize(context);

			Assert.IsTrue(noteCollection.Initialized);
		}

		[TestMethod]
		public void FirstNote()
		{
			var noteCollection = new StubNoteCollection();
			var sequence = new FixLengthSequence(noteCollection, 4, false, "id1");
			var context = new StubContext();

			sequence.Initialize(context);

			context.MetronomeTick = true;

			noteCollection.NextNote = new Note(5);

			var playingNote = sequence.GetNote(context);

			Assert.AreEqual(noteCollection.NextNote, playingNote.Note);
		}

	}

	internal class StubContext : IFlakyContext
	{
		public long Sample { get; set; }

		public int SampleRate => 44100;

		public int Beat { get; set; }

		public bool MetronomeTick { get; set; }

		public int BPM { get; set; } = 120;

		public TFactory Get<TFactory>() where TFactory : class
		{
			throw new NotImplementedException();
		}

		public TState GetOrCreateState<TState>(string id) where TState : class, new()
		{
			var key = (typeof(TState), id);

			if (!states.ContainsKey(key))
				states[key] = new TState();

			return (TState)states[key];
		}

		private readonly Dictionary<(Type, string), object> states
			= new Dictionary<(Type, string), object>();
	}

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
