using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Timers;


namespace ExperimentControl.Experiments
{
    /// <summary>
    /// 
    /// </summary>
    sealed class Experience1 : AControl
    {
        #region Attributes
        private System.Timers.Timer timerD;
        private System.Timers.Timer timerO;
        #endregion

        public Experience1() : base()
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
        /// <summary>
        /// Stop timers
        /// </summary>
        protected override void StopTimer()
        {
            timerD.Stop();
            timerO.Stop();
        }


        ///<summary>
        ///Start the experiment by starting the timers, turning the main lamp ON, putting the shutter and the red lamp at  LOW. It takes a picture with the Point Grey+ red Light.
        /// </summary>
        public override void Start()
            {
                Running = true;

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

