using Windows.Devices.Pwm;
using System;

namespace FrequencyGate
{
    public class Reference
    {
        // Set up the PWM outout pin used
        // const int PWM_OUTPUT_PIN = 23;
        public int PWMOutputPin { get; set; }

        // Frequency used by PWM controller 
        public int PWMFrequency { get; set; }

        PwmPin pwmRefFreq = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="PWMOutputPin">Output pin for Ref frequency</param>
        /// <param name="PWMFrequency">The Reference frequency</param>
        public Reference (int PWMOutputPin, int PWMFrequency )
        {
            PwmController pwmc = PwmController.GetDefault();
            pwmc.SetDesiredFrequency(PWMFrequency);

            pwmRefFreq = pwmc.OpenPin(PWMOutputPin);
            pwmRefFreq.SetActiveDutyCyclePercentage(0.5);

            Console.WriteLine("PWM max freq = " + pwmc.MaxFrequency.ToString());
            Console.WriteLine("PWM min freq = " + pwmc.MinFrequency.ToString());

        }

        public void RefFrequencyStart()
        {
            pwmRefFreq.Start();
        }
        
        public void RefFrequencyStop()
        {
            pwmRefFreq.Stop();
        }


        //public void CounterStart()
        //{
        //    gpcc.Start();
        //}

        //public void CounterStop()
        //{
        //    gpcc.Stop();
        //}

        //public double CounterReset()
        //{
        //    GpioChangeCount countZero = gpcc.Reset();
        //    return (countZero.Count);
        //}

        //public double CounterRead()
        //{
        //    GpioChangeCount countEnd = gpcc.Read();
        //    return (countEnd.Count);

        //}
    }
}