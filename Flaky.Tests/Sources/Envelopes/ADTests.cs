using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky.Tests
{
	[TestClass]
	public class ADTests
	{
		[TestMethod]
		public void NonZeroAttackNonZeroDecay()
		{
			var sequence = new FixLengthSequence(
				new SequentialNoteCollection("0", "id1"), 
				4, 
				false, 
				"id1");

			var ad = new AD(sequence, 0.1, 0.2);

			var context = new StubContext();

			sequence.Initialize(null, context);
			ad.Initialize(null, context);

			context.Beat = 0;

			AssertFloats.AreEqual(0, ad.Play(context).X);
			AssertFloats.AreEqual(0, ad.Play(context).Y);

			context.Sample = context.SampleRate / 10;

			AssertFloats.AreEqual(1, ad.Play(context).X);
			AssertFloats.AreEqual(1, ad.Play(context).Y);

			context.Sample = 2 * context.SampleRate / 10;

			AssertFloats.AreEqual(0.5f, ad.Play(context).X);
			AssertFloats.AreEqual(0.5f, ad.Play(context).Y);
		}
	}

	public static class AssertFloats
	{
		public static void AreEqual(float expected, float actual)
		{
			const float epsilon = 0.00001f;
			Assert.IsTrue(actual > expected - epsilon);
			Assert.IsTrue(actual < expected + epsilon);
		}
	}
}
