using System;
using System.Threading;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace ExperimentControl.ExperimentControl.Experiments
{
    /// <summary>
    /// Abstract class that define the base for the control the experiment
    /// </summary>
    public abstract class AControl : IControl
    {
        #region Attributes declaration 
        

        protected readonly IDigitalComponent shutterControl;
        protected readonly IDigitalComponent lampControl;
        protected readonly IDigitalComponent redLampControl;
        protected readonly Traverse traverse;
        protected readonly PtGreyCamera ptGreyCamera;
        protected  NikonCamera nikonCamera;
        private readonly System.Timers.Timer timerNikon;
        protected readonly LinearStage linearStage;



        public bool Running { get; protected set; } = false;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialize the control class by setting the timers, creating the channels and a Point Grey camera
        /// </summary>
        /// <exception cref="FormatNotRespectedException">Thrown when the setting file for the camera doesn't respect the expected format</exception>
        /// <exception cref="NoCameraDetectedException">Thrown when there isn't any camera detected</exception>
        public AControl()
        {

            SetTimer();

            timerNikon = new System.Timers.Timer
            {
                Interval = 2 * 3600 * 1000 + 15000, //we create a timer to reinitialize the nikon to avoid bugs
                Enabled = false,
                AutoReset = true
            };
            timerNikon.Elapsed += TimerNikon_Elapsed;

            timerNikon.Start();

            shutterControl = new ComponentControl(ConfigurationManager.AppSettings["Shutter"], "Shutter");
            lampControl = new RelayBoxComponent(ConfigurationManager.AppSettings["LampControl"], "Main lamp");
            redLampControl = new ComponentControl(ConfigurationManager.AppSettings["RedLampControl"], "Red Lamp");
            traverse = new Traverse();
            nikonCamera = new NikonCamera();

            try
            {
                PtGreyCameraSetting setting = new PtGreyCameraSetting("PtGreySetting.txt");
                ptGreyCamera = new PtGreyCamera(setting);
            }
            catch (FileNotFoundException)
            {
                ptGreyCamera = new PtGreyCamera();
            }
            using StreamWriter writer = new StreamWriter("log.txt", true);
            writer.WriteLine(ProtocolDescription());
        }
        /// <summary>
        /// Timer event handler to reinitialize the nikon to avoid bugs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerNikon_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                nikonCamera = new NikonCamera();
                #region Log
                DateTime date = DateTime.Now;
                string str = string.Format("{0}-{1}-{2}, {3:00}:{4:00}:{5:00}: Nikon camera reinitialized",
                date.Year,
                date.Month,
                date.Day,
                date.Hour,
                date.Minute,
                date.Second);
                using StreamWriter writer = new StreamWriter("log.txt", true);
                writer.WriteLine(str);
                #endregion
            }
            catch (NoCameraDetectedException ex)
            {
                #region Log
                DateTime date1 = DateTime.Now;
                string str1 = string.Format("{0}-{1}-{2}, {3:00}:{4:00}:{5:00}: ERROR: ",
                date1.Year,
                date1.Month,
                date1.Day,
                date1.Hour,
                date1.Minute,
                date1.Second) + ex.Message;
                using StreamWriter writer = new StreamWriter("log.txt", true);
                writer.WriteLine(str1);
                #endregion
                MessageBox.Show("No Nikon camera detected. Check if it is on.", "Error", MessageBoxButtons.OK);
            }
        }
        #endregion

        #region Methods
        ///<summary>
        ///Create timers
        ///</summary>
        protected abstract void SetTimer();
        /// <summary>
        /// Start the timers
        /// </summary>
        protected abstract void StartTimer();
        /// <summary>
        /// Stop timers
        /// </summary>
        protected abstract void StopTimer();

        protected abstract string ProtocolDescription();
      
        #region Assessors
        ///<summary>
        ///Return the state of the shutter
        ///True if open
        ///False if not
        /// </summary>
        public bool GetShutterState()
        {
            return shutterControl.State;
        }
        ///<summary>
        ///Return the state of the main lamp
        ///True if open
        ///False if not
        /// </summary>
        public bool GetLampState()
        {
            return lampControl.State;
        }
        ///<summary>
        ///Return the state of the red lamp
        ///True if open
        ///False if not
        /// </summary>
        public bool GetRedLampState()
        {
            return redLampControl.State;
        }
        #endregion

        ///<summary>
        ///Start the experiment by starting the timers, putting the shutter, the red and main lamp at  LOW.
        /// </summary>
        public virtual void Start()
        {
            Running = true;
            StartTimer();
            nikonCamera.PowerOn();
            #region Log
            DateTime date = DateTime.Now;
            string str = string.Format("{0}-{1}-{2}, {3:00}:{4:00}:{5:00}:",
            date.Year,
            date.Month,
            date.Day,
            date.Hour,
            date.Minute,
            date.Second) + " START: Beginning of the experiment";
            using (StreamWriter writer = new StreamWriter("log.txt", true))
            {
                writer.WriteLine(str);
            }
            #endregion
            lampControl.TurnOff();
            redLampControl.TurnOff();
            shutterControl.TurnOff();
        }


        ///<summary>
        ///Stop the experiment by putting everything at LOW, and stopping the timers
        /// </summary>
        public void Stop()
        {
            Running = false;
            nikonCamera.PowerOff();
            #region log

            DateTime date = DateTime.Now;
            string str = string.Format("{0}-{1}-{2}, {3:00}:{4:00}:{5:00}:",
            date.Year,
            date.Month,
            date.Day,
            date.Hour,
            date.Minute,
            date.Second) + " STOP: End of the experiment";
            using (StreamWriter writer = new StreamWriter("log.txt", true))
            {
                writer.WriteLine(str);
            }
            #endregion

            
            
            StopTimer();
            lampControl.TurnOff();
            redLampControl.TurnOff();
            shutterControl.TurnOff();
        }


        ///<summary>
        ///Take a picture of the whole tank.
        ///Take care of everything (light+camera).
        /// </summary>
        protected abstract void TankPicture();

        ///<summary>
        ///Open the laser shutter and take 10 picutres with the Nikon to visualize the flow and then close the shutter.
        /// </summary>
        protected void FlowVisualization()
        {
            shutterControl.TurnOn();
            Thread.Sleep(2000);//wait 2s to be sure it is stable
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    nikonCamera.Snap();

                    Thread.Sleep(900);  //need more reflexion on the value and timing accuracy
                }
            }
            catch (TriggerFailedException e)
            {
                #region Log
                DateTime date1 = DateTime.Now;
                string str1 = string.Format("{0}-{1}-{2}, {3:00}:{4:00}:{5:00}: ERROR: ",
                date1.Year,
                date1.Month,
                date1.Day,
                date1.Hour,
                date1.Minute,
                date1.Second) +e.Message;
                using StreamWriter writer = new StreamWriter("log.txt", true);
                writer.WriteLine(str1);
                #endregion
            }
           
            #region Log
            DateTime date = DateTime.Now;
            string str = string.Format("{0}-{1}-{2}, {3:00}:{4:00}:{5:00}: Flow Vizualization",
            date.Year,
            date.Month,
            date.Day,
            date.Hour,
            date.Minute,
            date.Second);
            using (StreamWriter writer = new StreamWriter("log.txt", true))
            {
                writer.WriteLine(str);
            }
            #endregion
            shutterControl.TurnOff();
           

        }

        #endregion
    }
}
