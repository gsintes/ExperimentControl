using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentControl.ExperimentControl
{
    class NonValidSerialNoException : Exception
    {
        public NonValidSerialNoException() : base("Serial number not valid")
        {

        }

    }
}
