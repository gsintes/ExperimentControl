using System;
using NationalInstruments.DAQmx;

namespace ExperimentControl
{
    abstract class ComponentControl
    {
        protected DOTask _control;
        protected bool _state = false;
        protected DigitalSingleChannelWriter writer;
        public bool State
        {
            get
            {
                return _state;
            }
        }

        public abstract void TurnOn();
        public abstract void TurnOff();

    }
}
