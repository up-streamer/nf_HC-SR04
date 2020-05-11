using System;
using System.Threading;
using Driver.HC_SR04;
using FrequencyGate;

namespace nf_HC_SR04
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("** nanoFramework Level Sensor with HC-SR04  on ESP32 DEV KITV1 Sample! **");

            Reference Freq = new Reference(22, 34300);
            Freq.RefFrequencyStart();
            Thread.Sleep(500);

            HC_SR04 Sensor = new HC_SR04(4, 2, 23)
            {
                EmptyDistance = 2000 // In milimeters
            };


            for (int i = 1; i < 200; i++)
            {
                Sensor.GetNewValues();

                Console.WriteLine("Level = " + Sensor.Level.ToString());
                Console.WriteLine("Distance = " + Sensor.Distance.ToString());
                Console.WriteLine("");
                
                Thread.Sleep(2000);
            }

        }
    }
}
