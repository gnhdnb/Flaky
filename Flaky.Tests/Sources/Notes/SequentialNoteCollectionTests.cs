﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flaky.Tests
{
	[TestClass]
	public class SequentialNoteCollectionTests
	{
		[TestMethod]
		public void SequentialNoteCollectionSequencing()
		{
			var collection = new SequentialNoteCollection("0-5dd2u", "parent1");
			var context = new StubContext();

			collection.Initialize(null, context);

			collection.Update(context);
			Assert.AreEqual(0, collection.GetNextNote().Number);
			collection.Update(context);
			Assert.AreEqual(Note.Silent, collection.GetNextNote());
			collection.Update(context);
			Assert.AreEqual(-19, collection.GetNextNote().Number);
			collection.Update(context);
			Assert.AreEqual(14, collection.GetNextNote().Number);
			collection.Update(context);
			Assert.AreEqual(0, collection.GetNextNote().Number);
		}
	}
}
