using System;
using System.Threading;
using Windows.Devices.Gpio;

namespace Driver.HC_SR04
{
    /// <summary>
    /// Class for controlling the HC-SR04 Ultrasonic Range detector
    /// Written by John E. Wilson
    /// Modified by Ricardo S. Santos
    /// </summary>
    /// Version 1.1 - 2012/04/03 - Corrected constructor pin documentation
    /// Free to use, please attribute credit
    /// Version 1.3 - 2015/02/02 - Modified object get/set declarations. Added level and volume calculation
    /// Version 1.4 - 2017/11/23 - Removed volume calculation to reduce INT load, transfered to UI.
    /// Version 1.5 - 2020/04/19 - Start porting to nanoFramework.

    public class HC_SR04
    {
        private GpioPin portOut;
        private GpioPin interIn;
        private GpioPin countPin;
        GpioChangeCounter gpcc = null;
        GpioChangeCount etick;
        private ulong beginTick;
        private ulong endTick;
        private long ticks;

        // Set up the counter pin used
        //const int COUNTER_INPUT_PIN = 22;
        // private readonly int CounterInputPin;
        public int CounterInputPin { get; private set; }
  

        /// <summary>
        /// Returns the library version number
        /// </summary>
        public double Version { get; private set; }

        /// <summary>
        /// The system latency (minimum number of ticks)
        /// This number will be subtracted off to find actual sound travel time
        /// </summary>
        public long LatencyTicks { private get; set; }

        /// <summary>
        /// Offset convert distance to level
        /// </summary>
        public int EmptyDistance { private get; set; }

        /// <summary>
        /// Factor o convert ticks to distance
        /// </summary>
        public double ConversionFactor { private get; set; }

        /// <summary>
        /// Ping, update error code, distance and level
        /// </summary>
        public void GetNewValues()
        {
            for (int retry = 1; retry < 3; retry++)
            {
                ticks = Ping();
                if (ticks > 0)                  //all good
                {
                    retry = 4;
                }
            }
            if (ticks == 0)            // Target too close. soft error
            {
                // throw new Exception("1");
            }
            else if (ticks == -1)           //No echo received. hard error
            {
                //  throw new Exception("2");
            }

            // Mudar error code 0 deve ser all good. Checar os Comsumers
            // i.e. LevelService e algum outro
            // Valid ticks
            Distance = (int)(ticks / ConversionFactor);

            Level = EmptyDistance - Distance;
        }

        public int Distance { get; private set; }

        public int Level { get; private set; }

        /* ********************************* */
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pinTrig">Pin connected to the HC-SR04 Trig pin</param>
        /// <param name="pinEcho">Pin connected to the HC-SR04 Echo pin</param>
        public HC_SR04(int pinTrig, int pinEcho, int CounterInputPin)
        {
            var gpioController = GpioController.GetDefault();

            // Initialise count pin by opening GPIO as input
            countPin = gpioController.OpenPin(CounterInputPin);
            countPin.SetDriveMode(GpioPinDriveMode.InputPullUp);

            // Create a Counter passing in the GPIO pin
            gpcc = new GpioChangeCounter(countPin);
            gpcc.Polarity = GpioChangePolarity.Rising;
            //{
            //    // Counter on raising edges
            //    Polarity = GpioChangePolarity.Rising
            //};
            gpcc.Start();
            //** Initialize HC-SR04 Related pins **
            portOut = gpioController.OpenPin(pinTrig);
            portOut.SetDriveMode(GpioPinDriveMode.Output);

            interIn = gpioController.OpenPin(pinEcho);
            interIn.SetDriveMode(GpioPinDriveMode.InputPullUp);
            
            interIn.ValueChanged += InterIn_OnInterrupt;
            
            LatencyTicks = 0L; // Was 6200L -> Aprox. 92.57mm (5287L latency) + 16mm (913L) offset from sensor grill;
            ConversionFactor = 1;//57.1056; // for Cm  (was 58.3) //1440.0;  for inches.
          
            Version = 1.6;
        }

        void InitGate()
        {

        }
        //  RefOsc = new Reference(); // (23, 22);
        
        //FrequencyGate.Reference();
        //  RefOsc.PWMFrequency = 10000;
        //RefOsc.RefFrequencyStart();
        /// <summary>
        /// Trigger a sensor reading
        /// Convert ticks to distance using TicksToDistance below
        /// </summary>
        /// <returns>Number of ticks it takes to get back sonic pulse</returns>
        long Ping()
        {
            // Reset Sensor
            endTick = 0L;
            GpioChangeCount btick = gpcc.Reset();
            Console.WriteLine("BeginTick After reset = " + beginTick.ToString());
            btick = gpcc.Read();
            Console.WriteLine("BeginTick = " + beginTick.ToString());

            beginTick = btick.Count;

            portOut.Write(GpioPinValue.High);
            // Thread.Sleep(1);
            // Trigger Sonic Pulse
            portOut.Write(GpioPinValue.Low);

            // Start Clock


            // Wait 1/20 second (this could be set as a variable instead of constant)
            Thread.Sleep(50);
           
            endTick = etick.Count;
            Console.WriteLine("EndTick = " + endTick.ToString());
            if (endTick > 0L)
            {
                // Calculate Difference
                long elapsed = (long) (endTick - beginTick);

                // Subtract out fixed overhead (interrupt lag, etc.)
                elapsed -= LatencyTicks;
                Console.WriteLine("Elapsed = " + elapsed.ToString());
                if (elapsed < 0L)
                {
                    elapsed = 0L;
                }

                // Return elapsed ticks
                return elapsed;
            }

            // Sonic pulse wasn't detected within 1/20 second
            return -1L;
        }

        /// <summary>
        /// This interrupt will trigger when detector receives back reflected sonic pulse       
        /// </summary>
        /// <param name="data1">Not used</param>
        /// <param name="data2">Not used</param>
        /// <param name="time">Transfer to endTick to calculated sound pulse travel time</param>
        void InterIn_OnInterrupt(object sender, GpioPinValueChangedEventArgs e)
        {
            // if (gpcc.IsStarted) { gpcc.Stop(); }

            // save the ticks when pulse was received back
            //if (e.Edge == GpioPinEdge.RisingEdge)
            //{
            //    beginTick = (long)gpcc.Read().Count; 
            //   // Console.WriteLine("begintick = " + beginTick.ToString());
            //};

            if (e.Edge == GpioPinEdge.FallingEdge)
            {
                 etick = gpcc.Read();
                //endTick = (long)gpcc.Read().Count;
                //Console.WriteLine("EndTick = " + endTick.ToString());
            };
            //GpioChangeCount etick = gpcc.Reset();
            // endTick = etick.Count;
        }
    }
}