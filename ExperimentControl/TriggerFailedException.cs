
using System;

namespace ExperimentControl.ExperimentControl
{
    class TriggerFailedException : Exception
    {
        public TriggerFailedException()  : base ("The triggering failed")
        {

        }
    }
}
