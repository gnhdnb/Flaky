using RestSharp;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace Flaky
{
	public class WebExternal : Source
	{
		internal WebExternal(string url, string id) : base(id)
		{
			this.url = url;
		}

		private string url;
		private State state;
		
		internal class State : IDisposable
		{
			private string url;
			private Timer timer;
			public float ExternalValue = 0;
			public float Value = 0;

			public void Init(string url)
			{
				this.url = url;

				if (timer == null)
				{
					timer = new Timer(UpdateValue, new object(), 100, 100);
				}
			}

			private void UpdateValue(object stateInfo)
			{
				try
				{
					int value = 0;
					var client = new RestClient(url);
					var request = new RestRequest(Method.GET);
					var response = client.Execute(request);

					if (response.IsSuccessful)
						int.TryParse(response.Content, out value);

					ExternalValue = ((float)value) / 100;
				}
				catch (Exception ex)
				{

				}
			}

			public void Dispose()
			{
				timer.Dispose();
			}
		}

		protected override Vector2 NextSample(IContext context)
		{
			var delta = state.ExternalValue - state.Value;

			state.Value += delta / 11050;

			return new Vector2(state.Value, state.Value);
		}

		protected override void Initialize(IContext context)
		{
			state = GetOrCreate<State>(context);
			state.Init(url);
		}

		public override void Dispose()
		{
			// nothing to dispose
		}
	}
}
