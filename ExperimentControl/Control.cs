using NationalInstruments.DAQmx;
using System;
using System.Threading;
using System.Timers;

namespace ExperimentControl
{
    /// <summary>
    /// Control the experiment
    /// </summary>
    class Control
    {
        #region Attributes declaration 

        private readonly DOTask shutterControl;
        private readonly DOTask lampRelayControl;
        private readonly DOTask redLampControl;

        private bool shutterState;
        private bool lampState;
        private bool redLampState;

        private PtGreyCamera ptGreyCamera;

        private System.Timers.Timer timerD;
        private System.Timers.Timer timerO;

        private int countDay;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialize the control class by setting the timers, creating the channels and a Point Grey camera
        /// </summary>
        public Control()
        {
            SetTimer();

            shutterState = false;
            lampState = false;
            redLampState = false;

            shutterControl = new DOTask("Dev1/port0/line0", "shutter");
            lampRelayControl = new DOTask("Dev1/port0/line1", "lampRelay");
           
            redLampControl = new DOTask("Dev1/port0/line2", "redLamp");
           
            ptGreyCamera = new PtGreyCamera();

        }
        #endregion

        #region Methods
        ///<summary>
        ///Create 2 timers one with a period of 12 hours, one with 1h.
        ///Timers disabled at the begining 
        ///Timers that re run automatically
        ///</summary>
        private void SetTimer()
        {
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
        ///<summary>
        ///Return the state of the shutter
        ///True if open
        ///False if not
        /// </summary>
        public bool GetShutterState()
        {
            return shutterState;
        }
        ///<summary>
        ///Return the state of the main lamp
        ///True if open
        ///False if not
        /// </summary>
        public bool GetLampState()
        {
            return lampState;
        }
        ///<summary>
        ///Return the state of the red lamp
        ///True if open
        ///False if not
        /// </summary>
        public bool GetRedLampState()
        {
            return redLampState;
        }

        ///<summary>
        ///Open the shutter by putting P0.0 at HIGH
        /// </summary>
        private void ShutterOn()
        {
            DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(shutterControl.Stream);
            writer.WriteSingleSampleSingleLine(true, true);
            shutterState = true;
        }
        ///<summary>
        ///Close the shutter by putting P0.0 at LOW
        /// </summary>
        private void ShutterOff()
        {
            DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(shutterControl.Stream);
            writer.WriteSingleSampleSingleLine(true, false);
            shutterState = false;
        }
        ///<summary>
        ///Turn off the red lamp by putting P0.2 at LOW
        /// </summary>
        /// <exception cref="RelayNotActiveException">Thrown when we try to control something on the relay but it not alimented on the VCC</exception>

        public void RedLampOff()
        {
            if (RelayVCC.State)
            {
                DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(redLampControl.Stream);
                writer.WriteSingleSampleSingleLine(true, false);
                redLampState = false;
            }
            else
            {
                throw new RelayNotActiveException();
            }
        }
        ///<summary>
        ///Turn on the red lamp by putting P0.2 at LOW
        /// </summary>
        /// <exception cref="RelayNotActiveException">Thrown when we try to control something on the relay but it not alimented on the VCC</exception>
        public void RedLampOn()
        {
            if (RelayVCC.State)
            {
                DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(redLampControl.Stream);
                writer.WriteSingleSampleSingleLine(true, true);
                redLampState = true;
            }
            else
            {
                throw new RelayNotActiveException();
            }
        }

        ///<summary>
        ///Turn on the main lamp by putting P0.1 at HIGH
        /// </summary>
        /// <exception cref="RelayNotActiveException">Thrown when we try to control something on the relay but it not alimented on the VCC</exception>
        private void LampOn()
        {
            if(RelayVCC.State)
            {
                DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(lampRelayControl.Stream);
                writer.WriteSingleSampleSingleLine(true, true);
                lampState = true;
            }
            else
            {
                throw new RelayNotActiveException();
            }
            
        }
        ///<summary>
        ///Turn off the main lamp by putting P0.1 at LOW
        /// </summary>
        /// <exception cref="RelayNotActiveException">Thrown when we try to control something on the relay but it not alimented on the VCC</exception>
        private void LampOff()
        {
            if (RelayVCC.State)
            {
                DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(lampRelayControl.Stream);
                writer.WriteSingleSampleSingleLine(true, false);
                lampState = false;
        }
             else
            {
                throw new RelayNotActiveException();
            }
        }

        ///<summary>
        ///Start the experiment by starting the timers, turning the main lamp ON, putting the shutter and the red lamp at  LOW. It takes a picture with the Point Grey+ red Light.
        /// </summary>
        public void Start()
        {
            RelayVCC.Activate();
            countDay = 0;
            timerD.Start();
            timerO.Start();
            LampOn();
            RedLampOff();
            ShutterOff();

            TankPicture();
            

        }

        ///<summary>
        ///Stop the experiment by putting everything at LOW, and stopping the timers
        /// </summary>
        public void Stop()
        {
            
            countDay = 0;
            timerD.Stop();
            timerO.Stop();
            LampOff();
            RedLampOff();
            ShutterOff();
            RelayVCC.Disactivate();
        }
       

        ///<summary>
        ///Take a picture of the whole tank using the point grey camera as configurated and the red lamp,
        ///Take care of everything (light+camera).
        /// </summary>
        private void TankPicture()
        {
            RedLampOn();
            Thread.Sleep(5000); //wait 5s to be sure it is stable

            ptGreyCamera.Snap("C:/Users/gs656local/Documents/Test/test.bmp");//argument to be changed
            Thread.Sleep(1000); // wait 1s to be sure the picture is taken
            RedLampOff();
        }
        ///<summary>
        ///Take a picture on the Nikon with the settings given on the (physical) camera
        /// </summary>
        private void NikonSnap()
        {
            
            //to be implemented
        }
        ///<summary>
        ///Open the laser shutter and take 10 picutres with the Nikon to visualize the flow and then close the shutter.
        /// </summary>
        private void FlowVisualization()
        {
            ShutterOn();
            Thread.Sleep(2000);//wait 2s to be sure it is stable

            for (int i = 0; i < 10; i++)
            {
                NikonSnap();
                Thread.Sleep(1000);  //need more reflexion on the value and timing accuracy
            }
            ShutterOff();
        }

        #region Timer EventHandler 
        /// <summary>
        /// Every 12h turn on or off the light for day/night cycle
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEventD(Object source, ElapsedEventArgs e)
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

        ///<summary>
        ///Every hour, as a response to the tick of timerO, we turn take a picture with the point grey and the red light, then 10 pictures with the nikon and the laser.
        ///</summary>
        private void OnTimedEventO(Object source, ElapsedEventArgs e)
        {
            TankPicture();
            Thread.Sleep(2000);
            FlowVisualization();

        }
        #endregion

        #endregion
    }
}
