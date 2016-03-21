using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Pwm;

namespace SamplePwmConsumer
{
    class PinsController
    {
        private PwmController _pwmController;


        public bool IsInitialized { get { return _pwmController != null; } }

        public async Task<bool> InitAsync(double frequency)
        {
            try
            {
                var controllers = await PwmController.GetControllersAsync(PwmSoftware.PwmProviderSoftware.GetPwmProvider());

                _pwmController = controllers?.FirstOrDefault();

                if (_pwmController != null)
                {
                    _pwmController.SetDesiredFrequency(frequency);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return false;
        }

        public PwmPin OpenPin(int pinNumber, double dutyCyclePercentage = 0)
        {
            if (_pwmController == null)
            {
                throw new InvalidOperationException();
            }
            var pin = _pwmController.OpenPin(pinNumber);

            pin?.SetActiveDutyCyclePercentage(dutyCyclePercentage);

            return pin;
        }
    }

}
