using System;
using NationalInstruments.DAQmx;

namespace ExperimentControl
{
    /// <summary>
    /// Enable the control of the relay by turning it on/off by sending a 5V DC to VCC.
    /// </summary>
    static class RelayVCC
    {

        private static readonly DOTask relay = new DOTask("Dev1/port0/line3", "relayVCC");
        private static bool s_state = false;
        static DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(relay.Stream);
        public static bool State
        {
            get { return s_state; }
        }
        /// <summary>
        /// Activate the control of the relay by sending 5V to VCC
        /// </summary>
        public static void Activate()
        {
            writer.WriteSingleSampleSingleLine(true, true);
            s_state = true;
        }
        /// <summary>
        /// Disactivate the control of the relay by sending 0V to VCC
        /// </summary>
        public static void Disactivate()
        {
            writer.WriteSingleSampleSingleLine(true, false);
            s_state = false;
        }
    }
}
