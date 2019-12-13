using System;
using System.IO;
using System.Timers;
using System.Threading;
using CameraControl.Devices;

namespace ExperimentControl.ExperimentControl.Experiments
{
    class Experiment5 : AControl
    {
        #region Attributes
        System.Timers.Timer timer;
        int count = 0;
        #endregion
        #region Constructor
        public Experiment5() : base()
        {
        linearStage = new LinearStage();
        }
        #endregion
        #region Methods
        /// <summary>
        /// Return a string with the description of the protocol
        /// </summary>
        /// <returns>Description of the protocol</returns>
        protected override string ProtocolDescription()
        {
            string res = "We are using protocol : Experiement5.\nCreated to control the test experiment in the small tank and in the dark, there is a 30s timer. We take one picture of the tank every 30s. The tank picutres are taken using the Point Grey camera and the red lamp. \n" +
                "Every 10min, we do a flow vizualisation with the Nikon and the laser. Using the linear stage.\n\n";
            return res;
        }
        /// <summary>
        /// Set one timer with a 30s interval
        /// </summary>
        protected override void SetTimer()
        {
            timer = new System.Timers.Timer
            {
                Interval = 30 * 1000,
                AutoReset = true,
                Enabled = false,
            };
            timer.Elapsed += Timer_Elapsed;
        }
        /// <summary>
        /// Event handler : lauch the minute routine in a new thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Thread thread = new Thread(Routine);
            thread.Start();
        }
        /// <summary>
        /// Define the minute routine : Every minute we take a picture of the tank, every 10min a flow visualisation
        /// </summary>
        private void Routine()
        {
            count++;
            if (linearStage.GetPosition() == 18)
            {
                TankPicture();
                if (count % 20 == 0)
                {
                    linearStage.Move(66);
                    FlowVisualization();
                    linearStage.Move(80);
                    FlowVisualization();
                    linearStage.Move(18);
                }
            }
            else
            {
                DateTime date = DateTime.Now;
                string fileName = Path.Combine(String.Format("TankPictures/im_{0}-{1:00}-{2:00}_{3:00}h{4:00}.bmp",
                    date.Year,
                    date.Month,
                    date.Day,
                    date.Hour,
                    date.Minute));
                fileName =
                        StaticHelper.GetUniqueFilename(
                            Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_", 0,
                            Path.GetExtension(fileName));
                // if file exist try to get a new filename to prevent file lost.
                // This is useful when camera is set to record in ram all filenames are thee same.
                if (File.Exists(fileName))
                {
                    fileName =
                        StaticHelper.GetUniqueFilename(
                            Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_", 0,
                            Path.GetExtension(fileName));
                }
                ptGreyCamera.Snap(fileName);
                #region Log
                string str = string.Format("{0}-{1}-{2}, {3:00}:{4:00}:{5:00}: SKIPPED tank picture",
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
            if (count>841
            )
            {
                Stop();
            }
                 
        }

        /// <summary>
        /// Start timers
        /// </summary>
        protected override void StartTimer()
        {
            timer.Start();
        }
        /// <summary>
        /// Stop timers
        /// </summary>
        protected override void StopTimer()
        {
            timer.Stop();
            linearStage.Stop();
        }
        /// <summary>
        /// Take a picture of the tank using the Point Grey camera
        /// </summary>
        protected override void TankPicture()
        {
            DateTime date = DateTime.Now;
            string fileName = Path.Combine(String.Format("TankPictures/im_{0}-{1:00}-{2:00}_{3:00}h{4:00}.bmp",
                date.Year,
                date.Month,
                date.Day,
                date.Hour,
                date.Minute));
            fileName =
                    StaticHelper.GetUniqueFilename(
                        Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_", 0,
                        Path.GetExtension(fileName));
            // if file exist try to get a new filename to prevent file lost.
            // This is useful when camera is set to record in ram all filenames are thee same.
            if (File.Exists(fileName))
            {
                fileName =
                    StaticHelper.GetUniqueFilename(
                        Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_", 0,
                        Path.GetExtension(fileName));
            }
            try
            {

                redLampControl.TurnOn();
                Thread.Sleep(1000); //wait 1s to be sure it is stable
                ptGreyCamera.Snap(fileName);
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
        /// <summary>
        /// Start the experiment and take a picture of the tank plus a visualisation of the flow.
        /// </summary>
        public override void Start()
        {
            linearStage.Start();
            base.Start();
            linearStage.SetVelocity(8m);
            linearStage.SetAcceleration(0.5m);
            linearStage.Move(18);

        }
        #endregion
    }
}