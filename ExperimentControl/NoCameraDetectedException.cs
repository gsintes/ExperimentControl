using System;


namespace ExperimentControl
{
    class NoCameraDetectedException : Exception
    {
        public NoCameraDetectedException() : base("No camera detected.")
        {
            }
    }
}
