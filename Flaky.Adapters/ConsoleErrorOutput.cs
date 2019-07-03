using System;
namespace Flaky
{
	public class ConsoleErrorOutput : IErrorOutput
	{
		public void WriteLine(string text)
		{
			Console.WriteLine(text);
		}
	}
}
