﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentControl.ExperimentControl.Experiments
{
    public enum Experiments { Exp1 = 1, Exp2, Exp3, Exp4, Exp5 };
    interface IControl
    {
        /// <summary>
        /// Start the experiment
        /// </summary>
        void Start();
        /// <summary>
        /// Stop the experiment
        /// </summary>
        void Stop();
        /// <summary>
        /// Return the shutter state
        /// </summary>
        /// <returns>True if open, false if closed</returns>
        bool GetShutterState();
        /// <summary>
        /// Return the main lamp state
        /// </summary>
        /// <returns>True if ON, false if OFF</returns>
        bool GetLampState();
        /// <summary>
        /// Return the red lamp state
        /// </summary>
        /// <returns>True if ON, false if OFF</returns>
        bool GetRedLampState();
        bool Running
        {
            get;
        }

    }
}
