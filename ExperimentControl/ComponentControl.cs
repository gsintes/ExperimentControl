﻿using System;
using NationalInstruments.DAQmx;
using System.IO;

namespace ExperimentControl.ExperimentControl
{
    /// <summary>
    /// Physical component control which is naturaly off (Turn On and OFF) 
    /// </summary>
    class ComponentControl : IDigitalComponent
    {
        #region Attributes

        private readonly DOTask control;
        private bool _state = false;
        private readonly DigitalSingleChannelWriter writer;
        private readonly string channelName;
        #endregion

        public bool State => _state;

        public ComponentControl(string port, string channelName)
        {
            control = new DOTask(port, channelName);
            writer = new DigitalSingleChannelWriter(control.Stream);
            this.channelName = channelName;
        }
        /// <summary>
        /// Turn on the component and update the state
        /// </summary>
        public virtual void TurnOn()
        {
            writer.WriteSingleSampleSingleLine(true, true);
            _state = true;
            DateTime date = DateTime.Now;
            string str = string.Format("{0}-{1}-{2}, {3:00}:{4:00}:{5:00}: ",
            date.Year,
            date.Month,
            date.Day, date.Hour, date.Minute,date.Second) + channelName + " turned on";
            using StreamWriter writer2 = new StreamWriter("log.txt", true);
            writer2.WriteLine(str);
        }
        /// <summary>
        /// Turn off the component and update the state
        /// </summary>
        public virtual void TurnOff()
        {
            writer.WriteSingleSampleSingleLine(true, false);
            _state = false;

            DateTime date = DateTime.Now;
            string str = string.Format("{0}-{1}-{2}, {3:00}:{4:00}:{5:00}: ",
            date.Year,
            date.Month,
            date.Day,
            date.Hour,
            date.Minute,
            date.Second) + channelName + " turned off";
            using StreamWriter writer2 = new StreamWriter("log.txt", true);
            writer2.WriteLine(str);
        }

    }
}
