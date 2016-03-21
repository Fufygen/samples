// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Background;
using Windows.Devices.Pwm;
using Windows.ApplicationModel.AppService;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;


// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace SamplePwmConsumer
{
    public sealed class StartupTask : IBackgroundTask
    {
        private const double PWM_FREQUENCY = 50d;
        private const double DUTY_CYCLE_PERCENTAGE = 0d;
        BackgroundTaskDeferral _deferral;

        HttpServer _httpServer;

        PinsController _controller;

        PwmPin _pin1;
        PwmPin _pin2;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            _controller = new PinsController();
            try
            {
                bool initialized = await _controller.InitAsync(PWM_FREQUENCY);
                if (initialized)
                {
                    _pin1 = _controller.OpenPin(19);
                    _pin1.Start();

                    _pin2 = _controller.OpenPin(20);
                    _pin2.Start();
                }

                _httpServer = new HttpServer(6000);
                _httpServer.MessageReceived += HttpServer_MessageReceived;
                _httpServer.StartServer();
            }
            catch (Exception ex)
            {
                _deferral.Complete();
                _deferral = null;
            }
        }

        private void HttpServer_MessageReceived(object sender, string message)
        {
            switch (message)
            {
                case "x":
                    _pin1.SetActiveDutyCyclePercentage(0.5);
                    break;
                case "z":
                    _pin1.SetActiveDutyCyclePercentage(1.0);
                    break;
                default:
                    _pin1.SetActiveDutyCyclePercentage(0.0);
                    break;
            }
        }

        ~StartupTask()
        {
            _pin1?.Stop();
            _pin2?.Stop();


            _pin1?.Dispose();
            _pin2?.Dispose();
            _httpServer?.Dispose();
        }
    }
}
