namespace ExperimentControl
{
    interface IDigitalComponent
    {
        bool State
        {
            get;
        }
        void TurnOn();
        void TurnOff();
    }
}
