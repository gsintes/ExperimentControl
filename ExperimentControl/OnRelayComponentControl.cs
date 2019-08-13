
namespace ExperimentControl
{
    /// <summary>
    /// Physical component plugged in the relay control (Turn On and OFF) 
    /// </summary>
    class OnRelayComponentControl : ComponentControl
    {
        public OnRelayComponentControl(string port, string channelName) : base(port,channelName)
        {

        }
        /// <summary>
        /// Turn on the device
        /// </summary>
        /// <exception cref="RelayNotActiveException"> Thrown when the relay is not active </exception>
        public override void TurnOn()
        {
            if (RelayVCC.State)
            {
                base.TurnOn();

            }
            else
            {
                throw new RelayNotActiveException();
            }
        }

        /// <summary>
        /// Turn off the device
        /// </summary>
        /// <exception cref="RelayNotActiveException"> Thrown when the relay is not active </exception>
        public override void TurnOff()
        {
            if (RelayVCC.State)
            {
                base.TurnOff();
            }
            else
            {
                throw new RelayNotActiveException();
            }
        }
    }
}
