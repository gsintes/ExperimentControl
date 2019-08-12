
using System;

namespace ExperimentControl
{
    class TriggerFailedException : Exception
    {
        public TriggerFailedException()  : base ("The triggering failed")
        {

        }
    }
}
