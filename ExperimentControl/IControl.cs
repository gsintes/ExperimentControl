using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentControl
{
    interface IControl
    {
        void Start();
        void Stop();
        bool GetShutterState();
        bool GetLampState();
        bool GetRedLampState();
        bool Running
        {
            get;
        }

    }
}
