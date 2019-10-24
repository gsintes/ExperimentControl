namespace ExperimentControl.ExperimentControl
{
    public interface IDigitalComponent
    {
        bool State
        {
            get;
        }
        /// <summary>
        /// TurnOn the component
        /// </summary>
        void TurnOn();
        /// <summary>
        /// TurnOff the component
        /// </summary>
        void TurnOff();
    }
}
