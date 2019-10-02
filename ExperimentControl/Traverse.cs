using NationalInstruments.DAQmx;
using System.Threading;
using System;

namespace ExperimentControl
{
    class Traverse
    {
        Task analogOutTask;
        public Traverse()
        {
            analogOutTask = new Task();
            analogOutTask.AOChannels.CreateVoltageChannel(
                "dev1/ao0",
                "",
                -5,
                5,
                AOVoltageUnits.Volts
                );
            analogOutTask.Control(TaskAction.Verify);
            
        }
        public void GenerateBitSync()
        {

            double freq = 1;
            int samplesPerCycle = 6;
            int cyclesPerBuffer = 10;
            double lowVal = 0;
            double highVal = 5;

            SquareWave square = new SquareWave(analogOutTask.Timing, freq, samplesPerCycle, cyclesPerBuffer, lowVal, highVal);
            analogOutTask.Timing.ConfigureSampleClock("", square.ResultingSampleClockRate, SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples, square.Data.Length);
            AnalogSingleChannelWriter writer = new AnalogSingleChannelWriter(analogOutTask.Stream);

            writer.WriteMultiSample(true, square.Data);

//            int p = Convert.ToInt32(1 / square.ResultingSampleClockRate);
//
//            for (int i = 0; i < square.Data.Length; i++)
//            {
//                double data = square.Data[i];
//                writer.WriteSingleSample(true, data);
//                Thread.Sleep(2000);
//            }
        }

    }
}