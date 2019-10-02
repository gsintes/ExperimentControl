using NationalInstruments.DAQmx;

namespace ExperimentControl
{
    class RedLamp
    {
        Task analogOutTask;
        private bool _state;
        private readonly AnalogSingleChannelWriter writer;
        const double VOLTAGE = 8;
        public RedLamp()
        {
            analogOutTask = new Task();
            analogOutTask.AOChannels.CreateVoltageChannel(
                "dev1/ao0",
                "",
                0,
                10,
                AOVoltageUnits.Volts
                );
            analogOutTask.Control(TaskAction.Verify);

            writer = new AnalogSingleChannelWriter(analogOutTask.Stream);
            writer.WriteSingleSample(true, 0); // Garanty that it is initially off
            _state = false;
        }

        public bool State => _state;

        public void TurnOn()
        {
            writer.WriteSingleSample(true, VOLTAGE);
            _state = true;
        }

        public void TurnOff()
        {
            writer.WriteSingleSample(true, 0); 
            _state = false;
        }


    }
}
