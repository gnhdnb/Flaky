using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal interface IExternalNoteSourceProcessor
	{
		PlayingNote GetNote(IContext context);
	}
}
