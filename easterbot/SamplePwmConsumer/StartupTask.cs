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
using Windows.Devices.Gpio;
using Windows.UI.Xaml;


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
        PwmPin _pin3;
        PwmPin _pin4;
        GpioPin _pin5;
        GpioController _gpioController;

        Double _fb = 0.0d;
        Double _lr = 0.0d;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            _controller = new PinsController();
            _gpioController = GpioController.GetDefault();

            bool initialized = await _controller.InitAsync(PWM_FREQUENCY);

            if (initialized)
            {
                _pin1 = _controller.OpenPin(6);
                _pin1.Start();

                _pin2 = _controller.OpenPin(13);
                _pin2.Start();

                _pin3 = _controller.OpenPin(19);
                _pin3.Start();

                _pin4 = _controller.OpenPin(26);
                _pin4.Start();

                _pin5 = _gpioController.OpenPin(4);
                _pin5.SetDriveMode(GpioPinDriveMode.Output);
                _pin5.Write(GpioPinValue.Low);

            }

            _httpServer = new HttpServer(6000);
            _httpServer.StartServer();

            _httpServer.MessageReceived += new EventHandler<byte[]>((s, m) => HandleMessage(m));

        }


        private void Stop()
        {
            _pin1.SetActiveDutyCyclePercentage(0);
            _pin2.SetActiveDutyCyclePercentage(0);
            _pin3.SetActiveDutyCyclePercentage(0);
            _pin4.SetActiveDutyCyclePercentage(0);
            _pin5.Write(GpioPinValue.Low);
        }

        private bool HandleMessage(byte[] bytes)
        {
            string message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            String cmd = message.Substring(0, 8);
            byte[] valbytes;
            Double ch1;
            Double ch2;

            switch (cmd)
            {
                case "/1/st\0\0\0":
                case "/1/fi/z\0":
                    _pin5.Write(GpioPinValue.Low);
                    break;
                case "/1/lr/z\0":
                    _lr = 0;
                    break;
                case "/1/fb/z\0":
                    _fb = 0;
                    break;
                case "/1/fi\0\0\0":
                    _pin5.Write(GpioPinValue.High);
                    break;
                case "/1/fb\0\0\0":
                    valbytes = bytes.Skip(12).Take(4).Reverse().ToArray();
                    _fb = BitConverter.ToSingle(valbytes, 0);
                    break;
                case "/1/lr\0\0\0":
                    valbytes = bytes.Skip(12).Take(4).Reverse().ToArray();
                    _lr = -BitConverter.ToSingle(valbytes, 0);
                    break;
                default:
                    break;
                    //return false;
            }
            if (_lr > 0.1)
            {
                ch1 = _fb;
                ch2 = -_fb * _lr;
            }
            else if (_lr < -0.1)
            {
                ch1 = _fb * _lr;
                ch2 = _fb;
            }
            else
            {
                ch1 = ch2 = _fb;
            }
            if (ch1 < 0)
            {
                _pin1.SetActiveDutyCyclePercentage(0);
                _pin2.SetActiveDutyCyclePercentage(-ch1);
            }
            else
            {
                _pin1.SetActiveDutyCyclePercentage(ch1);
                _pin2.SetActiveDutyCyclePercentage(0);
            }
            if (ch2 < 0)
            {
                _pin3.SetActiveDutyCyclePercentage(0);
                _pin4.SetActiveDutyCyclePercentage(-ch2);
            }
            else
            {
                _pin3.SetActiveDutyCyclePercentage(ch2);
                _pin4.SetActiveDutyCyclePercentage(0);
            }
            return true;
        }

        ~StartupTask()
        {
            _pin1?.Stop();
            _pin2?.Stop();
            _pin3?.Stop();
            _pin4?.Stop();

            _pin1?.Dispose();
            _pin2?.Dispose();
            _pin3?.Dispose();
            _pin4?.Dispose();

            _httpServer?.Dispose();
        }
    }
}
