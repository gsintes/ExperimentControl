using System;
using System.Threading;
using System.Timers;
using System.IO;
using System.Windows.Forms;

namespace ExperimentControl
{
    /// <summary>
    /// Control the experiment
    /// </summary>
    class Control
    {
        #region Attributes declaration 
        

        private readonly ComponentControl shutterControl;
        private readonly ComponentControl lampControl;
        private readonly ComponentControl redLampControl;

        private bool _Running = false;


        private readonly PtGreyCamera ptGreyCamera;
        private readonly NikonCamera nikonCamera;
        private System.Timers.Timer timerD;
        private System.Timers.Timer timerO;

        private int countDay;

        public bool Running { get => _Running; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initialize the control class by setting the timers, creating the channels and a Point Grey camera
        /// </summary>
        /// <exception cref="FormatNotRespectedException">Thrown when the setting file for the camera doesn't respect the expected format</exception>
        /// <exception cref="NoCameraDetectedException">Thrown when there isn't any camera detected</exception>
        public Control()
        {
 
            SetTimer();

            shutterControl = new ComponentControl("Dev1/port0/line0", "Shutter");
            lampControl = new ComponentControl("Dev1/port0/line1", "Main lamp");
            redLampControl = new ComponentControl("Dev1/port0/line2", "Red Lamp");
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
            _Running = true;
            
            countDay = 0;
            timerD.Start();
            timerO.Start();
          
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
                lampControl.TurnOn();
                redLampControl.TurnOff();
           
            shutterControl.TurnOff();

            TankPicture();
            FlowVisualization();
                    
        }


        ///<summary>
        ///Stop the experiment by putting everything at LOW, and stopping the timers
        /// </summary>
        public void Stop()
        {
            _Running = false;
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
            timerD.Stop();
            timerO.Stop();
            lampControl.TurnOff();
            redLampControl.TurnOff();
            shutterControl.TurnOff();



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
                ptGreyCamera.Snap(string.Format("TankPictures/im_{0}-{1}-{2}_{3:00}.bmp",
                date.Year,
                date.Month,
                date.Day, 
                date.Hour));
                Thread.Sleep(1000); // wait 1s to be sure the picture is taken
                redLampControl.TurnOff();
                
                #region Log
                string str = string.Format("{0}-{1}-{2}, {3:00}:{4:00}:{5:00}: TANK PICTURE taken by the Point Grey Camera",
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
            }
       
            catch (TriggerFailedException ex)
            {
                
                DateTime date = DateTime.Now;
                string str = string.Format("{0}-{1}-{2}, {3:00}:{4:00}:{5:00}: ERROR: ",
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
            catch (Exception ex) 
            {
                
                DateTime date = DateTime.Now;
                string str = string.Format("{0}-{1}-{2}, {3:00}:{4:00}:{5:00}: FATAL ERROR: ",
                date.Year,
                date.Month,
                date.Day,
                date.Hour,
                date.Minute,
                date.Second) + ex.Message; ;
                using (StreamWriter writer = new StreamWriter("log.txt", true))
                {
                    writer.WriteLine(str);
                }
                Stop();
            }

        }

        ///<summary>
        ///Open the laser shutter and take 10 picutres with the Nikon to visualize the flow and then close the shutter.
        /// </summary>
        private void FlowVisualization()
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
                lampControl.TurnOn();
                    
            }
            else
            {
                lampControl.TurnOff();
 
            }
            

        }

        ///<summary>
        ///Every hour, as a response to the tick of timerO, we turn take a picture with the point grey and the red light, then 10 pictures with the nikon and the laser.
        ///</summary>
        private void OnTimedEventO(Object source, ElapsedEventArgs e)
        {
            Thread thread = new Thread(HourRoutine);
            thread.Start();

        }
        private void HourRoutine()
        {
            TankPicture();
            Thread.Sleep(2000);
            FlowVisualization();
        }
        #endregion

        #endregion
    }
}
