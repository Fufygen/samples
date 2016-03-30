using System;
using System.Linq;
using System.Threading;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;

namespace SamplePwmConsumer
{
    sealed class HttpServer : IDisposable
    {
        private const uint BUFFER_SIZE = 256;
        private const int TIMER_PERIOD = 250; //250:250; worst case 499, best case 251
        private readonly int _port = 8000;
        private DatagramSocket _socket;
        private Stopwatch _sw = null;
        private Timer _timer;
        private long _timeout = 0;

        public HttpServer(int serverPort, long timeout = 0)
        {
            _socket = new DatagramSocket();
            _socket.MessageReceived += Socket_MessageReceived;
            _port = serverPort;
            _timeout = timeout;
        }

        public async void StartServer()
        {
            await _socket.BindServiceNameAsync(_port.ToString());
            if (_timeout != 0)
            {
                _timer = new Timer(timerEllapsed, null, 0, TIMER_PERIOD);
            }
        }

        public void Dispose()
        {
            _socket.Dispose();
            _timer.Dispose();
        }

        public event EventHandler<byte[]> MessageReceived;
        public event EventHandler<long> CommunicationTimedOut;

        private async void Socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            byte[] data = new byte[BUFFER_SIZE];
            IBuffer buffer = data.AsBuffer();
            IInputStream input = args.GetDataStream();
            await input.ReadAsync(buffer, BUFFER_SIZE, InputStreamOptions.Partial);
            MessageReceived?.Invoke(this, data.Take((int)buffer.Length).ToArray());
            _sw = _sw ?? Stopwatch.StartNew();
            _sw.Restart();
        }

        private void timerEllapsed(object state)
        {
            if (_sw == null) return;
            long ellapsed = _sw.ElapsedMilliseconds;
            if (ellapsed > _timeout)
            {
                CommunicationTimedOut?.Invoke(this, ellapsed);
                _sw.Reset();
            }
        }
    }
}
