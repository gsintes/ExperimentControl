using System;
using System.Threading;
using System.Timers;
using System.IO;
using System.Windows.Forms;

namespace ExperimentControl.Experiments
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
        protected readonly NikonCamera nikonCamera;


        protected int countDay;

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

            shutterControl = new ComponentControl("Dev1/port0/line0", "Shutter");
            lampControl = new RelayBoxComponent("Dev1/port0/line1", "Main lamp");
            redLampControl = new ComponentControl("Dev1/port0/line2","Red Lamp");
            traverse = new Traverse();
            try
            {
                nikonCamera = new NikonCamera();
            }
            catch (NoCameraDetectedException)
            {
                MessageBox.Show("No Nikon camera detected. Check if it is on.", "Error", MessageBoxButtons.OK);
            }


            try
            {
                PtGreyCameraSetting setting = new PtGreyCameraSetting("PtGreySetting.txt");
                ptGreyCamera = new PtGreyCamera(setting);
            }
            catch (FileNotFoundException)
            {
                ptGreyCamera = new PtGreyCamera();
            }
        }
        #endregion

        #region Methods
        ///<summary>
        ///Create timers
        ///</summary>
        protected abstract void SetTimer();
        /// <summary>
        /// Stop timers
        /// </summary>
        protected abstract void StopTimer();
      
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
        ///Start the experiment by starting the timers, turning the main lamp ON, putting the shutter and the red lamp at  LOW. It takes a picture with the Point Grey+ red Light.
        /// </summary>
        public abstract void Start();


        ///<summary>
        ///Stop the experiment by putting everything at LOW, and stopping the timers
        /// </summary>
        public void Stop()
        {
            Running = false;
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

            
            countDay = 0;
            StopTimer();
            lampControl.TurnOff();
            redLampControl.TurnOff();
            shutterControl.TurnOff();
        }


        ///<summary>
        ///Take a picture of the whole tank using the point grey camera as configurated and the red lamp,
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
                using (StreamWriter writer = new StreamWriter("log.txt", true))
                {
                    writer.WriteLine(str1);
                }
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
