using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentControl
{
    class RelayNotActiveException : Exception 
    {
        public RelayNotActiveException() : base("Relay not activated by the VCC, can't control")
        {

        }
    }
}
