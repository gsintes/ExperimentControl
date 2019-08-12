using NationalInstruments.DAQmx;
using System;
using System.Threading;
using System.Timers;

namespace ExperimentControl
{
    class Control
    {
        #region Variables declaration 
        private Task shutterControl;
        private Task lampRelayControl;
        private Task redLampControl;

        private bool shutterState;
        private bool lampState;
        private bool redLampState;

        private System.Timers.Timer timerD;
        private System.Timers.Timer timerO;

        private int countDay;
        #endregion

        #region Constructors
        public Control()
        {
            SetTimer();

            shutterState = false;
            lampState = false;
            redLampState = false;

            shutterControl = new Task();
            _ = shutterControl.DOChannels.CreateChannel(
                "Dev1/port0/line0",
                "shutter",
                ChannelLineGrouping.OneChannelForAllLines
                );

            lampRelayControl = new Task();
            _ = lampRelayControl.DOChannels.CreateChannel(
                "Dev1/port0/line1",
                "lampRelay",
                ChannelLineGrouping.OneChannelForAllLines
                );

            redLampControl = new Task();
            _ = redLampControl.DOChannels.CreateChannel(
                "Dev1/port0/line2",
                "redLamp",
                ChannelLineGrouping.OneChannelForAllLines
                );

        }
        #endregion

        #region Methods

        private void SetTimer()
        {
            ///<summary>
            ///Create 2 timers one with a period of 12 hours, one with 1h.
            ///Timers disabled at the begining 
            ///Timers that re run automatically
            ///</summary>

            // Create 2 timer with a two second interval.
            this.timerD = new System.Timers.Timer();
            timerD.Interval = 12 * 3600 * 1000;// 12h in ms

            this.timerO = new System.Timers.Timer();
            timerO.Interval = 1 * 3600 * 1000;// 1h in ms

            // Hook up the Elapsed event for the timer. 
            timerD.Elapsed += OnTimedEventD;
            timerD.AutoReset = true;
            timerD.Enabled = false;

            timerO.Elapsed += OnTimedEventO;
            timerO.AutoReset = true;
            timerO.Enabled = false;

        }

        public bool GetShutterState()
        {
            ///<summary>
            ///Return the state of the shutter
            ///True if open
            ///False if not
            /// </summary>
            return shutterState;
        }

        public bool GetLampState()
        {
            ///<summary>
            ///Return the state of the main lamp
            ///True if open
            ///False if not
            /// </summary>
            return lampState;
        }
        public bool GetRedLampState()
        {
            ///<summary>
            ///Return the state of the red lamp
            ///True if open
            ///False if not
            /// </summary>
            return redLampState;
        }
        private void ShutterOn()
        {
            ///<summary>
            ///Open the shutter by putting P0.0 at HIGH
            /// </summary>
            DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(shutterControl.Stream);
            writer.WriteSingleSampleSingleLine(true, true);
            shutterState = true;
        }

        private void ShutterOff()
        {
            ///<summary>
            ///Close the shutter by putting P0.0 at LOW
            /// </summary>
            DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(shutterControl.Stream);
            writer.WriteSingleSampleSingleLine(true, false);
            shutterState = false;
        }

        public void RedLampOff()
        {
            ///<summary>
            ///Turn off the red lamp by putting P0.2 at LOW
            /// </summary>
            DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(redLampControl.Stream);
            writer.WriteSingleSampleSingleLine(true, false);
            redLampState = false;
        }

        public void RedLampOn()
        {
            ///<summary>
            ///Turn on the red lamp by putting P0.2 at LOW
            /// </summary>
            DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(redLampControl.Stream);
            writer.WriteSingleSampleSingleLine(true, true);
            redLampState = true;
        }

        private void LampOn()
        {
            ///<summary>
            ///Turn on the main lamp by putting P0.1 at HIGH
            /// </summary>
            DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(lampRelayControl.Stream);
            writer.WriteSingleSampleSingleLine(true, true);
            lampState = true;
        }
        private void LampOff()
        {
            ///<summary>
            ///Turn off the red lamp by putting P0.1 at LOW
            /// </summary>
            DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(lampRelayControl.Stream);
            writer.WriteSingleSampleSingleLine(true, false);
            lampState = false;
        }
        public void Start()
        {
            ///<summary>
            ///Start the experiment by starting the timers, turning the main lamp ON, putting the shutter and the red lamp at  LOW. It takes a picture with the Point Grey+ red Light.
            /// </summary>
            countDay = 0;
            timerD.Start();
            timerO.Start();
            LampOn();
            RedLampOff();
            ShutterOff();

            TankPicture();

        }

        public void Stop()
        {
            ///<summary>
            ///Stop the experiment by putting everything at LOW, and stopping the timers
            /// </summary>
            countDay = 0;
            timerD.Stop();
            timerO.Stop();
            LampOff();
            RedLampOff();
            ShutterOff();

        }

        private void OnTimedEventD(Object source, ElapsedEventArgs e) //every 12h turn on or off the light for day/night cycle
        {
            countDay++;
            if (countDay % 2 == 0)
            {
                LampOn();
            }
            else
            {
                LampOff();
            }
        }

        private void PtGSnap()
        {
            /// <summary>
            /// Take a picture with the camera point Grey with the settings given in the configuration of the camera
            /// </summary>
            //to be implemented
        }
        private void TankPicture()
        {
            ///<summary>
            ///Take a picture of the whole tank using the point grey camera as configurated and the red lamp,
            ///Take care of everything (light+camera).
            /// </summary>
            RedLampOn();
            Thread.Sleep(5000); //wait 5s to be sure it is stable

            PtGSnap();
            Thread.Sleep(1000); // wait 1s to be sure the picture is taken
            RedLampOff();
        }
        private void NikonSnap()
        {
            ///<summary>
            ///Take a picture on the Nikon with the settings given on the (physical) camera
            /// </summary>
            /// 
            //to be implemented
        }
        private void FlowVisualization()
        {
            ///<summary>
            ///Open the laser shutter and take 10 picutres with the Nikon to visualizer the flow and then close the shutter.
            /// </summary>


            ShutterOn();
            Thread.Sleep(2000);//wait 2s to be sure it is stable

            for (int i = 0; i < 10; i++)
            {
                NikonSnap();
                Thread.Sleep(1000);  //need more reflexion on the value and timing accuracy
            }
            ShutterOff();
        }
        private void OnTimedEventO(Object source, ElapsedEventArgs e)
        {
            ///<summary>
            ///Every hour, as a response to the tick of timerO, we turn take a picture with the point grey and the red light, then 10 pictures with the nikon and the laser.
            ///</summary>

            TankPicture();
            FlowVisualization();

        }

        #endregion
    }
}
