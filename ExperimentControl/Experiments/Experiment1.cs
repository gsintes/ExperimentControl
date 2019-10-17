using System;
using System.IO;
using System.Threading;
using System.Timers;



namespace ExperimentControl.ExperimentControl.Experiments
{
    /// <summary>
    /// Experiment with : a 12h day/night cycle,
    /// every hour we do a flow visualisation with 10 Nikon pictures with 1s gap,
    /// a tank picture with the point grey.
    /// </summary>
    sealed class Experiment1 : AControl
    {
        #region Attributes
        private System.Timers.Timer timerD;
        private System.Timers.Timer timerO;
        private int countDay = 0;
        #endregion

        /// <summary>
        /// Initialize the experiment 
        /// </summary>
        public Experiment1() : base()
        {
        }



        #region Methods
        ///<summary>
        ///Create 2 timers one with a period of 12 hours, one with 1h.
        ///Timers disabled at the begining 
        ///Timers that re run automatically
        ///</summary>
        protected override void SetTimer()
        {
            this.timerD = new System.Timers.Timer();
            this.timerO = new System.Timers.Timer();

            timerD.Interval = 12 * 60 * 60 * 1000;
            timerO.Interval = 60* 60 * 1000;

            timerD.Elapsed += OnTimedEventD;
            timerO.Elapsed += OnTimedEventO;

            timerD.Enabled = false;
            timerO.Enabled = false;

            timerD.AutoReset = true;
            timerO.AutoReset = true;
        }
        /// <summary>
        /// Stop timers
        /// </summary>
        protected override void StopTimer()
        {
            timerD.Stop();
            timerO.Stop();
        }
        /// <summary>
        /// Start the timers
        /// </summary>
        protected override void StartTimer()
        {
            timerD.Start();
            timerO.Start();
        }

        ///<summary>
        ///Start the experiment by starting the timers, turning the main lamp ON, putting the shutter and the red lamp at  LOW. It takes a picture with the Point Grey+ red Light.
        /// </summary>
        public override void Start() 
        {
            base.Start();
            TankPicture();
            FlowVisualization();

        }

        ///<summary>
        ///Take a picture of the whole tank using the point grey camera as configurated and the red lamp,
        ///Take care of everything (light+camera).
        /// </summary>
        protected override void TankPicture()
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
                using StreamWriter writer = new StreamWriter("log.txt", true);
                writer.WriteLine(str);
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
                using StreamWriter writer = new StreamWriter("log.txt", true);
                writer.WriteLine(str);
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

        /// <summary>
        /// Start he hour routine into an other thread
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEventO(Object source, ElapsedEventArgs e)
        {
            Thread thread = new Thread(HourRoutine);
            thread.Start();
        }
        ///<summary>
        ///Every hour, as a response to the tick of timerO, we turn take a picture with the point grey and the red light, then 10 pictures with the nikon and the laser.
        ///</summary>
        private void HourRoutine()
        {
            TankPicture();
            Thread.Sleep(2000);
            FlowVisualization();
        }

        protected override string ProtocolDescription()
        {
            string res = "We are using protocol : Experiement1.\nCreated to control the main experiment, there are two timers: 12h and 1h.Every 12h, we switch between day and night.Every hour," +
                " we take first a picture of the tank with the Point Grtey, then a visualisation of the flow, ie 10 pictures of the laser sheet with the Nikon with 1s pause. The is no movement of the tank ";
            return res;
        }

        #endregion
        #endregion

    }

}

