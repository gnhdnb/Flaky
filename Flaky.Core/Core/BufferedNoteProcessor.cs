using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	public class BufferedNoteProcessor : IExternalNoteSourceProcessor
	{
		private const int bufferSize =
			SeparateThreadProcessor.bufferSize * (SeparateThreadProcessor.readBuffersCount + 1);

		private readonly IFlakyNoteSource source;
		private readonly PlayingNote[] buffer = new PlayingNote[bufferSize];

		private long writeSample = -1;

		internal BufferedNoteProcessor(IFlakyNoteSource source)
		{
			this.source = source;
		}

		public PlayingNote GetNote(IContext context)
		{
			if (context.Sample > writeSample)
			{
				writeSample = context.Sample;
				var note = source.GetNoteInCurrentThread(context);
				buffer[writeSample % bufferSize] = note;
				return note;
			}
			else
			{
				return buffer[context.Sample % bufferSize];
			}
		}
	}
}
