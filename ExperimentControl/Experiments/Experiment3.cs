using System;
using System.Threading;
using System.IO;

namespace ExperimentControl.ExperimentControl.Experiments
{
    /// <summary>
    /// Experiemnt where we just use the red lamp and the nikon camera.
    /// We take a picture of the tank every 30s.
    /// </summary>
    sealed class Experiment3 : AControl
    {
        private  System.Timers.Timer timer30;
        /// <summary>
        /// Initialize the experiment 
        /// </summary>
        public Experiment3() : base()
        { }

        protected override string ProtocolDescription()
        {
            string res = "We are using protocol : Experiement3.\nCreated to control the test experiment in the small tank and in the dark, there is a 30s timer. We take one picture of the tank every 30s. The tank picutres are taken using the Nikon camera and the red lamp.\n\n";
            return res;
        }
        /// <summary>
        /// Start the experiment by setting evything plus taking a first picture
        /// </summary>
        public override void Start()
        {
            base.Start();
            TankPicture();
        }
        protected override void SetTimer()
        {
            timer30 = new System.Timers.Timer
            {
                Interval = 30 * 1000,
                AutoReset= true,
                Enabled = false
            };
            timer30.Elapsed += Timer30_Elapsed;
            
        }

        private void Timer30_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TankPicture();
        }


        protected override void StartTimer()
        {
            timer30.Start();
        }

        protected override void StopTimer()
        {
            timer30.Stop();
        }

        protected override void TankPicture()
        {
            redLampControl.TurnOn();
            Thread.Sleep(500);
            nikonCamera.Snap();
            DateTime date = DateTime.Now;
            #region Log
            string str = string.Format("{0}-{1}-{2}, {3:00}:{4:00}:{5:00}: TANK PICTURE taken",
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
            Thread.Sleep(2000);
            redLampControl.TurnOff();
        }
    }
}
