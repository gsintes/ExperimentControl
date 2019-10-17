using System;


namespace ExperimentControl.ExperimentControl
{
    class SoftwareTriggerNotSupportedException : Exception
    {
        public SoftwareTriggerNotSupportedException() : base("The camera does'nt support software trigger.")
        {

        }
    }
}
