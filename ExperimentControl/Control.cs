using System;
using System.Threading;
using System.Timers;
using System.IO;
using CameraControl.Devices;

namespace ExperimentControl
{
    /// <summary>
    /// Control the experiment
    /// </summary>
    class Control
    {
        #region Attributes declaration 
        

        private readonly ComponentControl shutterControl;
        private readonly OnRelayComponentControl lampControl;
        private readonly OnRelayComponentControl redLampControl;


        private readonly PtGreyCamera ptGreyCamera;
        private readonly NikonCamera nikonCamera;
        private System.Timers.Timer timerD;
        private System.Timers.Timer timerO;

        private int countDay;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialize the control class by setting the timers, creating the channels and a Point Grey camera
        /// </summary>
        /// <exception cref="FormatNotRespectedException">Thrown when the setting file for the camera doesn't respect the expected format</exception>
        /// <exception cref="NoCameraDetectedException">Thrown when there isn't any camera detected</exception>
        public Control()
        {
            try
            {
                SetTimer();

                shutterControl = new ComponentControl("Dev1/port0/line0", "Shutter");
                lampControl = new OnRelayComponentControl("Dev1/port0/line1", "Main lamp");

                redLampControl = new OnRelayComponentControl("Dev1/port0/line2", "Red Lamp");
                CameraSetting setting = new CameraSetting("PtGreySetting.txt");
                ptGreyCamera = new PtGreyCamera(setting);
                nikonCamera = new NikonCamera();
            }
            catch (FileNotFoundException)
            {
                ptGreyCamera = new PtGreyCamera();
            }
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
        public void Start()
        {
            
            RelayVCC.Activate();
            countDay = 0;
            timerD.Start();
            timerO.Start();
            try
            {
                #region Log
                DateTime date = DateTime.Now;
                string str = string.Format("{0}-{1}-{2},{3}:{4}:{5}:",
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
                lampControl.TurnOn();
                redLampControl.TurnOff();
            }
            catch (RelayNotActiveException ex)
            {
                DateTime date = DateTime.Now;
                string str = string.Format("{0}-{1}-{2},{3}:{4}: ERROR:",
                date.Year,
                date.Month,
                date.Day,date.Hour,date.Minute) + ex.Message;
                using (StreamWriter writer = new StreamWriter("log.txt", true))
                {
                    writer.WriteLine(str);
                }

            }

            shutterControl.TurnOff();

            TankPicture();       
                    
                    
        }


        ///<summary>
        ///Stop the experiment by putting everything at LOW, and stopping the timers
        /// </summary>
        public void Stop()
        {
            #region log

            DateTime date = DateTime.Now;
            string str = string.Format("{0}-{1}-{2},{3}:{4}:{5}:",
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

            try
            {
                countDay = 0;
                timerD.Stop();
                timerO.Stop();
                lampControl.TurnOff();
                redLampControl.TurnOff();
                shutterControl.TurnOff();
                        
            }
            catch (RelayNotActiveException)
            {
                RelayVCC.Activate();
                countDay = 0;
                timerD.Stop();
                timerO.Stop();
                lampControl.TurnOff();
                redLampControl.TurnOff();
                shutterControl.TurnOff();

            }
            RelayVCC.Disactivate();
        }
       

        ///<summary>
        ///Take a picture of the whole tank using the point grey camera as configurated and the red lamp,
        ///Take care of everything (light+camera).
        /// </summary>
        private void TankPicture()
        {
            try
            {
                
                redLampControl.TurnOn();
                Thread.Sleep(5000); //wait 5s to be sure it is stable
                DateTime date = DateTime.Now;
                ptGreyCamera.Snap(string.Format("TankPictures/im_{0}-{1}-{2}_{3}.bmp",
                date.Year,
                date.Month,
                date.Day, 
                date.Hour));//argument to be changed
                Thread.Sleep(1000); // wait 1s to be sure the picture is taken
                redLampControl.TurnOff();
            }
            catch (RelayNotActiveException ex)
            {
                DateTime date = DateTime.Now;
                string str = string.Format("{0}-{1}-{2},{3}:{4}:{5}: ERROR:",
                date.Year,
                date.Month,
                date.Day, 
                date.Hour, 
                date.Minute,
                date.Second) + ex.Message;
                using (StreamWriter writer = new StreamWriter("log.txt", true))
                {
                    writer.WriteLine(str);
                }

            }
            catch (TriggerFailedException ex)
            {
                DateTime date = DateTime.Now;
                string str = string.Format("{0}-{1}-{2},{3}:{4}:{5}: ERROR:",
                date.Year,
                date.Month,
                date.Day, date.Hour, date.Minute,date.Second) + ex.Message;
                using (StreamWriter writer = new StreamWriter("log.txt", true))
                {
                    writer.WriteLine(str);
                }
            }
            finally
            {
                DateTime date = DateTime.Now;
                string str = string.Format("{0}-{1}-{2},{3}:{4}:{5}: FATAL ERROR:",
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
                Stop();
            }

        }
        ///<summary>
        ///Take a picture on the Nikon with the settings given on the (physical) camera
        /// </summary>
        
        ///<summary>
        ///Open the laser shutter and take 10 picutres with the Nikon to visualize the flow and then close the shutter.
        /// </summary>
        private void FlowVisualization()
        {
            int count = 0;
            DateTime date = DateTime.Now;
            string prefix = string.Format("im_{0}-{1}-{2},{3}:{4}", date.Year,
                date.Month,
                date.Day,
                date.Hour,
                date.Minute);
            shutterControl.TurnOn();
            Thread.Sleep(2000);//wait 2s to be sure it is stable
            
            for (int i = 0; i < 10; i++)
            {

                string filename = StaticHelper.GetUniqueFilename(prefix, count, ".extension"); //choose extension
                nikonCamera.Snap(filename);
                Thread.Sleep(1000);  //need more reflexion on the value and timing accuracy
            }
            shutterControl.TurnOff();
        }

        #region Timer EventHandler 
        /// <summary>
        /// Every 12h turn on or off the light for day/night cycle
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEventD(Object source, ElapsedEventArgs e)
        {
            try
            {
                countDay++;
                if (countDay % 2 == 0)
                {
                    lampControl.TurnOn();
                    
                }
                else
                {
                    lampControl.TurnOff();
 
                }
            }
            catch (RelayNotActiveException ex)
            {
                DateTime date = DateTime.Now;
                string str = string.Format("{0}-{1}-{2},{3}:{4}: ERROR:",
                date.Year,
                date.Month,
                date.Day, date.Hour, date.Minute) + ex.Message;
                using (StreamWriter writer = new StreamWriter("log.txt", true))
                {
                    writer.Write(str);
                }
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
