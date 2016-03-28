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
        private DatagramSocket _socket;

        public HttpServer(int serverPort)
        {
            _socket = new DatagramSocket();
            _socket.MessageReceived += Socket_MessageReceived;
            _port = serverPort;
        }

        public async void StartServer()
        {
            await _socket.BindServiceNameAsync(_port.ToString());
        }


        public void Dispose()
        {
            _socket.Dispose();
        }

        public event EventHandler<byte[]> MessageReceived;

        private async void Socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        { 
            byte[] data = new byte[BUFFER_SIZE];
            IBuffer buffer = data.AsBuffer();
            IInputStream input = args.GetDataStream();
            await input.ReadAsync(buffer, BUFFER_SIZE, InputStreamOptions.Partial);
            MessageReceived?.Invoke(this, data.Take((int)buffer.Length).ToArray());
        }
    }
}
