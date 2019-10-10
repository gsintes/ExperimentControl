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
        public Experiment2() : base()
        { }

        public override void Start()
        {
            base.Start();
            TankPicture();
        }

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

        protected override void StartTimer()
        {
            timer10.Start();
            timer1.Start();
        }

        protected override void StopTimer()
        {
            timer10.Stop();
            timer1.Stop();
        }

        protected override void TankPicture()
        {
            redLampControl.TurnOn();
            Thread.Sleep(500);
            nikonCamera.Snap();
            Thread.Sleep(2000);
            redLampControl.TurnOff();
        }

        #region EventHandlers
        private void OnTimedEvent10(Object source, ElapsedEventArgs e)
        {
            count ++;
            Thread thread = new Thread(TankPicture);
            thread.Start();
        }
        private void OnTimedEvent1(Object source, ElapsedEventArgs e)
        {
            if (count < 6 && count % 10 !=0)
            {
                Thread thread = new Thread(TankPicture);
                thread.Start();
            }
        }

        #endregion
    }
}
