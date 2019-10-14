using System;
using System.Timers;
using System.Threading;


namespace ExperimentControl.Experiments
{   
    /// <summary>
    /// Experiemnt where we just use the red lamp and the nikon camera.
    /// We take a picture of the tank every minutes during the first hour and then every 10min.
    /// </summary>
    sealed class Experiment2 : AControl
    {
        #region Attributes
        private System.Timers.Timer timer1;
        private System.Timers.Timer timer10;
        private int count = 1;

        #endregion
        /// <summary>
        /// Initialize the experiment 
        /// </summary>
        public Experiment2() : base()
        { }
        /// <summary>
        /// Start the experiment by setting evything plus taking a first picture
        /// </summary>
        public override void Start()
        {
            base.Start();
            TankPicture();
        }
        ///<summary>
        ///Create timers, one with 1min and one with 10mins
        ///</summary>
        protected override void SetTimer()
        {
            timer10 = new System.Timers.Timer();
            timer1 = new System.Timers.Timer();

            timer10.Interval = 10 * 60 * 1000;
            timer1.Interval = 60 * 1000; 

            timer10.Elapsed += OnTimedEvent10;
            timer1.Elapsed += OnTimedEvent1;

            timer1.Enabled = false;
            timer10.Enabled = false;

            timer1.AutoReset = true;
            timer10.AutoReset = true;

        }
        /// <summary>
        /// Start the timers
        /// </summary>
        protected override void StartTimer()
        {
            timer10.Start();
            timer1.Start();
        }
        /// <summary>
        /// Stop the timers
        /// </summary>
        protected override void StopTimer()
        {
            timer10.Stop();
            timer1.Stop();
        }
        /// <summary>
        /// Take a picture of the tank with the nikon and the red light
        /// </summary>
        protected override void TankPicture()
        {
            redLampControl.TurnOn();
            Thread.Sleep(500);
            nikonCamera.Snap();
            Thread.Sleep(2000);
            redLampControl.TurnOff();
        }

        #region EventHandlers
        /// <summary>
        /// Event handler for every 10mins, takes a picture of the tank
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent10(Object source, ElapsedEventArgs e)
        {
            count ++;
            Thread thread = new Thread(TankPicture);
            thread.Start();
        }
        /// <summary>
        /// Event handler for every minutes, takes a picture of the tank during the first hour
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent1(Object source, ElapsedEventArgs e)
        {
            if (count < 6 && count % 10 !=0)
            {
                Thread thread = new Thread(TankPicture);
                thread.Start();
            }

        }

        protected override string ProtocolDescription()
        {
            string res = "We are using protocol : Experiement2.\nCreated to control the test experiment in the small tank and in the dark, there are two timers: 1min and 10min. During the first hour we take one picture of the tank every minute and then every 10min. The tank picutres are taken using the Nikon camera and the red lamp.";
            return res;
        }

        #endregion
    }
}
