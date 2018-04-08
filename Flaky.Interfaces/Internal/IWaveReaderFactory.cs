﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flaky
{
	internal interface IWaveReaderFactory
	{
		IWaveReader Create(string fileName);

		IMultipleWaveReader Create(string folder, string pack);
	}
}
