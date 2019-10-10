namespace ExperimentControl
{
    public interface IDigitalComponent
    {
        bool State
        {
            get;
        }
        void TurnOn();
        void TurnOff();
    }
}
