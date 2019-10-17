using System;


namespace ExperimentControl.ExperimentControl
{
    class NoCameraDetectedException : Exception
    {
        public NoCameraDetectedException() : base("No camera detected.")
        {
            }
    }
}
