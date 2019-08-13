using System;
using NationalInstruments.DAQmx;

namespace ExperimentControl
{
    /// <summary>
    /// Physical component control (Turn On and OFF) 
    /// </summary>
    class ComponentControl
    {
        #region Attributes

        private readonly DOTask control;
        private bool _state = false;
        private readonly DigitalSingleChannelWriter writer;
        #endregion

        public bool State => _state;

        public ComponentControl(string port, string channelName)
        {
            control = new DOTask(port, channelName);
            writer = new DigitalSingleChannelWriter(control.Stream);
        }
        /// <summary>
        /// Turn on the component and update the state
        /// </summary>
        public virtual void TurnOn()
        {
            writer.WriteSingleSampleSingleLine(true, true);
            _state = true;
        }
        /// <summary>
        /// Turn off the component and update the state
        /// </summary>
        public virtual void TurnOff()
        {
            writer.WriteSingleSampleSingleLine(true, false);
            _state = false;
        }

    }
}
