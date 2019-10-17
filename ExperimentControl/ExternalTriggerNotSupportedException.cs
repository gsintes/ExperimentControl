using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentControl.ExperimentControl
{
    class ExternalTriggerNotSupportedException : Exception
    {
        public ExternalTriggerNotSupportedException() : base("External trigger not supported by the camera.")
        {

        }
    }
}
