using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flaky
{
	public interface IWebClient
	{
		Stream Get(string url);
		Stream Post(string url, object body);
	}
}
