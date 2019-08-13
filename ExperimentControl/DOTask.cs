using System;
using NationalInstruments.DAQmx;

namespace ExperimentControl
{
    ///<summary>
    ///A digital output task with the physical channel given.
    /// Inherit from Task
    /// </summary>
    class DOTask : Task
    {

        public DOTask(string port, string channelName) : base()
        {
            _ = DOChannels.CreateChannel(
                port,
                channelName,
                ChannelLineGrouping.OneChannelForAllLines
                );
        }
    }
}
