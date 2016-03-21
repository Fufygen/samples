using System;
using System.Collections.Generic;
using Windows.ApplicationModel.AppService;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace SamplePwmConsumer
{
    sealed class HttpServer : IDisposable
    {
        private const uint BUFFER_SIZE = 256;
        private readonly int _port = 8000;
        private StreamSocketListener _listener;

        public HttpServer(int serverPort)
        {
            _listener = new StreamSocketListener();
            _listener.Control.KeepAlive = true;
            _listener.Control.NoDelay = true;
            _listener.ConnectionReceived += async (s, e) => { await ProcessRequestAsync(e.Socket); };

            _port = serverPort;
        }

        public async void StartServer()
        {
            await _listener.BindServiceNameAsync(_port.ToString());
        }


        public void Dispose()
        {
            _listener.Dispose();
        }

        public event EventHandler<string> MessageReceived;

        private async Task ProcessRequestAsync(StreamSocket socket)
        {
            byte[] data = new byte[BUFFER_SIZE];
            IBuffer buffer = data.AsBuffer();
            using (IInputStream input = socket.InputStream)
            {
                await input.ReadAsync(buffer, BUFFER_SIZE, InputStreamOptions.Partial);
                string msg = Encoding.UTF8.GetString(data, 0, (int)buffer.Length);

                MessageReceived?.Invoke(this, msg);
            }
        }
    }
}
