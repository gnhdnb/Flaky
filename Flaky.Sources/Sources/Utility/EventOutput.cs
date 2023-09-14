using System;
using System.Threading;
using System.Linq;
using System.IO;

namespace Flaky
{
    internal interface IEventOutput
    {
        void Initialize(IFlakyContext context);
        void Push(DateTime timestamp, int eventIndex);
    }

    public class WebEventOutput : IEventOutput
    {
        private class State : IDisposable
        {
            private struct WebEvent 
            {
                public DateTime timestamp;
                public int eventIndex;
            }

            private IWebClient webClient;
            private IErrorOutput errorOutput;
            private Thread worker;

            private string url;

            private bool disposing = false;
            private bool initialized = false;

            private const int maxBufferSize = 1024;
            private readonly WebEvent[] outputBuffer = new WebEvent[maxBufferSize];
            private volatile int bufferCounter = 0;
            private ManualResetEvent disposingEvent = new ManualResetEvent(false);

            public void Initialize(IFlakyContext context, string url)
            {
                if (initialized)
                    return;

                initialized = true;

                this.url = url;

                webClient = context.Get<IWebClient>();
                errorOutput = context.Get<IErrorOutput>();

                worker = new Thread(UploadLoop);

                worker.Start();
            }

            public void Push(DateTime timestamp, int eventIndex)
            {
                outputBuffer[bufferCounter] = new WebEvent 
                {
                    timestamp = timestamp,
                    eventIndex = eventIndex
                };

                bufferCounter++;
                bufferCounter = bufferCounter % maxBufferSize;
            }

            private void UploadLoop()
            {
                while (!disposing)
                {
                    if (bufferCounter > 0)
                    {
                        var currentCounter = bufferCounter;
                        var request = outputBuffer.Take(currentCounter).ToList();
                        bufferCounter = 0;

                        try
                        {
                            using (var response = webClient.Post(url, request))
                            using (var reader = new StreamReader(response))
                                reader.ReadToEnd();
                        }
                        catch (Exception ex)
                        {
                            errorOutput.WriteLine(ex.ToString());
                        }
                    }

                    disposingEvent.WaitOne(10);
                }
            }

            void IDisposable.Dispose()
            {
                disposing = true;
                disposingEvent.Set();
                worker.Join();
            }
        }

        private readonly string url;
        private State state;

        public WebEventOutput(string url)
        {
            this.url = url;
        }

        void IEventOutput.Initialize(IFlakyContext context)
        {
            this.state = context.GetOrCreateState<State>(url);

            this.state.Initialize(context, url);
        }

        public void Push(DateTime timestamp, int eventIndex)
        {
            this.state.Push(timestamp, eventIndex);
        }
    }
}