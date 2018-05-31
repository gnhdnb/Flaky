using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	interface IFlakyNoteSource : INoteSource
	{
		void SetExternalProcessor(IExternalNoteSourceProcessor processor);
		PlayingNote GetNoteInCurrentThread(IContext context);
	}
}
