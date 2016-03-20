// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Text;
using Windows.ApplicationModel.Background;
using Windows.Devices.Pwm;
using Windows.ApplicationModel.AppService;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;


// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace SamplePwmConsumer
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral deferral;
        HttpServer httpServer;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();

            httpServer = new HttpServer(6000);
            httpServer.StartServer();

        }

        ~StartupTask()
        {
            httpServer.Dispose();
        }
    }

    public sealed class HttpServer : IDisposable
    {
        private const uint BufferSize = 256;
        private int port = 8000;
        private StreamSocketListener listener;

        private PwmPin motorPin;
        private PwmPin secondMotorPin;
        private PwmController pwmController;
        double RestingPulseLegnth = 0;

        public HttpServer(int serverPort)
        {
            listener = new StreamSocketListener();
            listener.Control.KeepAlive = true;
            listener.Control.NoDelay = true;

            port = serverPort;
            listener.ConnectionReceived += async (s, e) => { await ProcessRequestAsync(e.Socket); };
        }

        public async void StartServer()
        {
            pwmController = (await PwmController.GetControllersAsync(PwmSoftware.PwmProviderSoftware.GetPwmProvider()))[0];
            pwmController.SetDesiredFrequency(50);
            motorPin = pwmController.OpenPin(19);
            motorPin.SetActiveDutyCyclePercentage(RestingPulseLegnth);
            motorPin.Start();
            secondMotorPin = pwmController.OpenPin(20);
            secondMotorPin.SetActiveDutyCyclePercentage(RestingPulseLegnth);
            secondMotorPin.Start();

            await listener.BindServiceNameAsync(port.ToString());
        }


        public void Dispose()
        {
            motorPin.Stop();
            secondMotorPin.Stop();
            listener.Dispose();
        }

        private async Task ProcessRequestAsync(StreamSocket socket)
        {
            // this works for text only
            StringBuilder request = new StringBuilder();
            byte[] data = new byte[BufferSize];
            IBuffer buffer = data.AsBuffer();
            uint dataRead = BufferSize;
            using (IInputStream input = socket.InputStream)
            {
                while (dataRead == BufferSize)
                {
                    await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                    string msg = Encoding.UTF8.GetString(data, 0, (int)buffer.Length);
                    switch (msg)
                    {
                        case "x":
                            motorPin.SetActiveDutyCyclePercentage(0.5);
                            break;
                        case "z":
                            motorPin.SetActiveDutyCyclePercentage(1.0);
                            break;
                        default:
                            motorPin.SetActiveDutyCyclePercentage(0.0);
                            break;
                    }
                    dataRead = buffer.Length;
                }
            }
        }
    }
}
