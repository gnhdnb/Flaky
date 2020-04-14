using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flaky
{
	public class WebClient : IWebClient
	{
		public Stream Get(string url)
		{
			var client = new RestClient();

			var request = new RestRequest(url, Method.GET);

			var response = client.Execute(request);

			return new MemoryStream(response.RawBytes);
		}
	}
}
