using NationalInstruments.DAQmx;
using System.Threading;
using System;
using System.Configuration;

namespace ExperimentControl
{
    public  enum Direction { Up, Down };
    public class Traverse
    {
        #region Attributes
        private readonly DOTask dirTask;
        private readonly DigitalSingleChannelWriter dirWriter;
        private readonly DOTask pulse;
        private readonly DigitalSingleChannelWriter pulseWriter;
        private const double STEP = 250e-4; // in mm
        #endregion

        #region Constructors
        public Traverse()
        {
            dirTask = new DOTask(ConfigurationManager.AppSettings["TravDir"], "dir");
            dirWriter = new DigitalSingleChannelWriter(dirTask.Stream);

            pulse = new DOTask(ConfigurationManager.AppSettings["TravPulse"], "pulse");
            pulseWriter = new DigitalSingleChannelWriter(pulse.Stream);
            direction = Direction.Down;

            pulseWriter.WriteSingleSampleSingleLine(true, false);
            dirWriter.WriteSingleSampleSingleLine(true, false);

        }
        #endregion

        #region Acessors
        public Direction direction { get; private set; }
        #endregion

        #region Methods

        /// <summary>
        /// Set the direction of the traverse 
        /// </summary>
        private void SetDirection(Direction dir)
        {
            direction = dir;
            if (dir == Direction.Up)
            {
                dirWriter.WriteSingleSampleSingleLine(true, true);
            }
            else
            {
                dirWriter.WriteSingleSampleSingleLine(true, false);

            }

        }
      

        /// <summary>
        /// Do a step of the traverse in the current direction
        /// </summary>
        private void Step()
        {
            pulseWriter.WriteSingleSampleSingleLine(true, true);
            pulseWriter.WriteSingleSampleSingleLine(true, false);
        }
        /// <summary>
        /// Move the traverse 
        /// </summary>
        /// <param name="nbStep">Number of steps the traverse will do</param>
        /// <param name="dir">Direction of the moving traverse (up or down) </param>
        /// <param name="pause">Time of pause between each step </param>
        private void MoveStep(int nbStep, Direction dir, int pause )
        {
            SetDirection(dir);
            for (int i=0; i<nbStep; i++)
            {
                Step();
                Thread.Sleep(pause);
            }
        }
        /// <summary>
        /// Move the traverse
        /// </summary>
        /// <param name="distance">Distance on which the traverse should move, in millimeter </param>
        /// <param name="dir">Direction of the moving traverse (up or down)</param>
        /// <param name="speed">Speed at which the traverse should move, in millimeter/second</param>
        public void Move(double distance, Direction dir, double speed)
        {
            int nbStep = Convert.ToInt32(distance / STEP);
            int pause = Convert.ToInt32(1000 * STEP / speed);
            MoveStep(nbStep, dir, pause);
        }

        #endregion
    }
}