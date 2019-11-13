using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Thorlabs.MotionControl.Benchtop.StepperMotorCLI;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.GenericMotorCLI.ControlParameters;
using System.Configuration;

namespace ExperimentControl.ExperimentControl
{
    public class LinearStage
    {
        #region Attributes declaration
        private readonly string serialNo = ConfigurationManager.AppSettings["LinStageSerialNo"];
        private readonly BenchtopStepperMotor device;
        private readonly StepperMotorChannel channel;
        #endregion

        public LinearStage()
        {
            // Tell the device manager to get the list of all devices connected to the computer
            DeviceManagerCLI.BuildDeviceList();

            //Test serialNo
            if (!TestSerial(serialNo))
            {
                throw new NonValidSerialNoException();
            }

            // Create the BenchtopStepperMotor device
            device = BenchtopStepperMotor.CreateBenchtopStepperMotor(serialNo);
            if (device == null)
            {
                throw new NonValidSerialNoException();
            }

            // Open a connection to the device.
            try
            {
                device.Connect(serialNo);
            }
            catch (Exception)
            {
                // Connection failed
                Console.WriteLine("Failed to open device {0}", serialNo);
                return;
            }

            // Get the correct channel - channel 1
            channel = device.GetChannel(1);
            if (channel == null)
            {
                // Connection failed
                Console.WriteLine("Channel unavailable {0}", serialNo);
                return;
            }

            // Wait for the device settings to initialize - timeout 5000ms
            if (!channel.IsSettingsInitialized())
            {
                try
                {
                    channel.WaitForSettingsInitialized(5000);
                }
                catch (Exception)
                {
                    Console.WriteLine("Settings failed to initialize");
                }
            }
        }

        #region Methods
        /// <summary>
        /// Return the position the stage
        /// </summary>
        /// <returns></returns>
        public decimal GetPosition()
        {
            return channel.Position;
        }
        /// <summary>
        /// Stop the stage and turn it off.
        /// </summary>
        public void Stop()
        {
            channel.StopPolling();
            device.Disconnect(true);
        }
        /// <summary>
        /// Start the linear stage and home it
        /// </summary>
        public void Start()
        {
            // Start the device polling
            // The polling loop requests regular status requests to the motor to ensure the program keeps track of the device.
            channel.StartPolling(250);
            // Needs a delay so that the current enabled state can be obtained
            Thread.Sleep(500);
            // Enable the channel otherwise any move is ignored 
            channel.EnableDevice();
            // Needs a delay to give time for the device to be enabled
            Thread.Sleep(500);

            // Call LoadMotorConfiguration on the device to initialize the DeviceUnitConverter object required for real world unit parameters
            //  - loads configuration information into channel
            // Use the channel.DeviceID "40xxxxxx-1" to get the channel 1 settings. This is different to the serial number
            _ = channel.LoadMotorConfiguration(channel.DeviceID);
            Home();
        }
        /// <summary>
        /// Set the max velocity of the stage
        /// </summary>
        /// <param name="velocity">The velocity in mm/s, We need v smaller than 12mm/s</param>
        public void SetVelocity(decimal velocity)
        {
            if (velocity < 12) 
            {
                VelocityParameters velPars = channel.GetVelocityParams();
                velPars.MaxVelocity = velocity;
                channel.SetVelocityParams(velPars); 
            }
            else
            {
                #region Log
                DateTime date1 = DateTime.Now;
                string str1 = string.Format("{0}-{1}-{2}, {3:00}:{4:00}:{5:00}: ERROR: Velocity of {6:00} is too high for the linear stage, v set to 12mm/s ",
                date1.Year,
                date1.Month,
                date1.Day,
                date1.Hour,
                date1.Minute,
                date1.Second,
                velocity);
                using StreamWriter writer = new StreamWriter("log.txt", true);
                writer.WriteLine(str1);
                #endregion
                VelocityParameters velPars = channel.GetVelocityParams();
                velPars.MaxVelocity = 12m;
                channel.SetVelocityParams(velPars);
            }
        }
        /// <summary>
        /// Set the acceleration of the stage
        /// </summary>
        /// <param name="a">The acceleration in mm/s2, We need a smaller than 10mm/s2</param>
        public void SetAcceleration(decimal a)
        {
            if (a < 10)
            {
                VelocityParameters velPars = channel.GetVelocityParams();
                velPars.Acceleration = a;
                channel.SetVelocityParams(velPars);
            }
            else
            {
                #region Log
                DateTime date1 = DateTime.Now;
                string str1 = string.Format("{0}-{1}-{2}, {3:00}:{4:00}:{5:00}: ERROR: Acceleration of {6:00} is too high for the linear stage, a set to 10mm/s2 ",
                date1.Year,
                date1.Month,
                date1.Day,
                date1.Hour,
                date1.Minute,
                date1.Second,
                a);
                using StreamWriter writer = new StreamWriter("log.txt", true);
                writer.WriteLine(str1);
                #endregion
                VelocityParameters velPars = channel.GetVelocityParams();
                velPars.Acceleration = 10m;
                channel.SetVelocityParams(velPars);
            }
        }
        /// <summary>
        /// Test the validity of the serial number
        /// </summary>
        /// <param name="serialNo">serialNo to be tested (it is a string)</param>
        /// <returns></returns>
        private bool TestSerial(string serialNo)
        {
            // Get available Benchtop Stepper Motor and check our serial number is correct - by using the device prefix
            char pref = serialNo[0];
            List<string> serialNumbers;
            switch (pref)
            {
                case '4':
                    serialNumbers = DeviceManagerCLI.GetDeviceList(BenchtopStepperMotor.DevicePrefix40);
                    break;
                case '7':
                    serialNumbers = DeviceManagerCLI.GetDeviceList(BenchtopStepperMotor.DevicePrefix70);
                    break;
                default:
                    return false;
                    

            }
            if (!serialNumbers.Contains(serialNo))
            {
                // The requested serial number is not a BSC203 or is not connected
                return false;
            }
            return true;
        }
        /// <summary>
        /// Home the device
        /// </summary>
        private void Home( )
        { 
            channel.Home(60000);  
        }
        /// <summary>
        /// Move the device to the given position
        /// </summary>
        /// <param name="position">Position in mm</param>
        public void Move(decimal position)
        {
            try
            {
                channel.MoveTo(position, 60000);
            }
            catch (Exception ex)
            {
                #region Log
                DateTime date1 = DateTime.Now;
                string str1 = string.Format("{0}-{1}-{2}, {3:00}:{4:00}:{5:00}: ERROR: ",
                date1.Year,
                date1.Month,
                date1.Day,
                date1.Hour,
                date1.Minute,
                date1.Second) + ex.Message+ " Failed to move to position, deviced ask to be homed";
                using StreamWriter writer = new StreamWriter("log.txt", true);
                writer.WriteLine(str1);
                #endregion
                Home();
            }
        }

        #endregion
    }
}
