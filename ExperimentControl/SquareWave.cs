using NationalInstruments.DAQmx;

namespace ExperimentControl
{ 
    public class SquareWave
    {
        private double desiredSampleClockRate;
        private double resultingFrequency;
        private readonly double lowValue;
        private readonly double highValue;

        public SquareWave(Timing timingObject, double frequency, int samplesPerCycle, int cyclesPerBuffer, double lowValue,  double highValue)
        {
            this.lowValue = lowValue;
            this.highValue = highValue;
            timingObject.SampleTimingType = SampleTimingType.SampleClock;
            int samplesPerBuffer = samplesPerCycle * cyclesPerBuffer;
            desiredSampleClockRate = frequency * samplesPerCycle;
            timingObject.SampleClockRate = desiredSampleClockRate;
            ResultingSampleClockRate = timingObject.SampleClockRate;
            resultingFrequency = ResultingSampleClockRate / (samplesPerCycle);
            Data = GeneratePoints(ResultingSampleClockRate, samplesPerBuffer, samplesPerCycle);

        }



        public double[] Data { get; }

        public double ResultingSampleClockRate { get; }


        private double[] GeneratePoints(double sampleClockRate, int samplesPerBuffer, int samplesPerCycle)
        {
            double amplitude = highValue - lowValue;
            int i;
            int j;
            double deltaT = 1 / sampleClockRate;
            int mid = samplesPerCycle / 2;
            double[] dataVal = new double[samplesPerBuffer];

            for (i = 0; i < samplesPerBuffer; i++)
            {
                j = i % samplesPerCycle;

                double d = lowValue + (amplitude * (j / mid)); // integer division
                dataVal[i] = d;
            }

            return dataVal;

        }
    }
}
