using System;
using System.Threading;
using System.Timers;
using System.IO;
namespace ExperimentControl.ExperimentControl.Experiments
{
    /// <summary>
    /// Experiment in the dark, tank picture every minute with the Pt Grey, flow visualization every 10min with the Nikon
    /// </summary>
    class Experiment4 : AControl
    {
        #region Attributes
        System.Timers.Timer timer;
        int count = 0;
        #endregion
        #region Constructor
        public Experiment4() : base()
        {

        }
        #endregion
        #region Methods
        /// <summary>
        /// Return a string with the description of the protocol
        /// </summary>
        /// <returns>Description of the protocol</returns>
        protected override string ProtocolDescription()
        {
            string res = "We are using protocol : Experiement4.\nCreated to control the test experiment in the small tank and in the dark, there is a 1min timer. We take one picture of the tank every minute. The tank picutres are taken using the Point Grey camera and the red lamp. \n"+
                "Every 10min, we do a flow vizualisation with the Nikon and the laser.\n\n";
            return res;
        }
        /// <summary>
        /// Set one timer with a one minute Interval
        /// </summary>
        protected override void SetTimer()
        {
            timer = new System.Timers.Timer
            {
                Interval = 60 * 1000,
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
            Thread thread = new Thread(MinuteRoutine);
            thread.Start();
        }
        /// <summary>
        /// Define the minute routine : Eery minute we take a picture of the tank, every 10min a flow visualisation
        /// </summary>
        private void MinuteRoutine()
        {
            count++;
            TankPicture();
            if (count % 10 == 0)
            {
                traverse.Move(41, Direction.Down, 20);
                FlowVisualization();
                traverse.Move(9, Direction.Down, 20);
                FlowVisualization();
                traverse.Move(50, Direction.Up, 20);
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
        }
        /// <summary>
        /// Take a picture of the tank using the Point Grey camera
        /// </summary>
        protected override void TankPicture()
        {
            try
            {

                redLampControl.TurnOn();
                Thread.Sleep(5000); //wait 5s to be sure it is stable
                DateTime date = DateTime.Now;
                ptGreyCamera.Snap(string.Format("TankPictures/im_{0}-{1:00}-{2:00}_{3:00}h{4:00}.bmp",
                date.Year,
                date.Month,
                date.Day,
                date.Hour,
                date.Minute));
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
        /// <summary>
        /// Start the experiment and take a picture of the tank plus a visualisation of the flow.
        /// </summary>
        public override void Start()
        {
            base.Start();
            traverse.Move(300, Direction.Up, 20);
            TankPicture();
            traverse.Move(41, Direction.Down, 20);
            FlowVisualization();
            traverse.Move(9, Direction.Down, 20);
            FlowVisualization();
            traverse.Move(50, Direction.Up, 20);
        }
        #endregion
    }
}
