using System;


namespace ExperimentControl
{
    class SoftwareTriggerNotSupportedException : Exception
    {
        public SoftwareTriggerNotSupportedException() : base("The camera does'nt support software trigger.")
        {

        }
    }
}
