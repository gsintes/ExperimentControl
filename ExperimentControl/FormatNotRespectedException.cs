using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentControl
{ 
    class FormatNotRespectedException: Exception
    {
        public FormatNotRespectedException() : base("File format not respected.")
        {

        }
    }
}
