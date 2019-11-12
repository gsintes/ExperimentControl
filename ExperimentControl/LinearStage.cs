using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Thorlabs.MotionControl.Benchtop.StepperMotorCLI;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.GenericMotorCLI;
using Thorlabs.MotionControl.GenericMotorCLI.ControlParameters;
using Thorlabs.MotionControl.GenericMotorCLI.AdvancedMotor;
using Thorlabs.MotionControl.GenericMotorCLI.Settings;
using System.Configuration;

namespace ExperimentControl.ExperimentControl
{
    class LinearStage
    {
        #region Attributes declaration
        private static bool _taskComplete;
        private static ulong _taskID;
        #endregion

        public LinearStage()
        {

            decimal position = 0m;
            decimal velocity = 5m;
            // get the test BSC203 serial number
            string serialNo = ConfigurationManager.AppSettings["LinStageSerialNo"];


            // Tell the device manager to get the list of all devices connected to the computer
            DeviceManagerCLI.BuildDeviceList();



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
                    serialNumbers = new List<string>();
                    break;

            }

         
            if (!serialNumbers.Contains(serialNo))
            {
                // The requested serial number is not a BSC203 or is not connected
                throw new NonValidSerialNoException();
            }

            // Create the BenchtopStepperMotor device
            BenchtopStepperMotor device = BenchtopStepperMotor.CreateBenchtopStepperMotor(serialNo);
            if (device == null)
            {
                throw new NonValidSerialNoException();
            }

            // Open a connection to the device.
            try
            {
                Console.WriteLine("Opening device {0}", serialNo);
                device.Connect(serialNo);
            }
            catch (Exception)
            {
                // Connection failed
                Console.WriteLine("Failed to open device {0}", serialNo);
                return;
            }

            // Get the correct channel - channel 1
            StepperMotorChannel channel = device.GetChannel(1);
            if (channel == null)
            {
                // Connection failed
                Console.WriteLine("Channel unavailable {0}", serialNo);
                Console.ReadKey();
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
            // Use the channel.DeviceID "70xxxxxx-1" to get the channel 1 settings. This is different to the serial number
            MotorConfiguration motorConfiguration = channel.LoadMotorConfiguration(channel.DeviceID);

            // Not used directly in example but illustrates how to obtain device settings
            ThorlabsBenchtopStepperMotorSettings currentDeviceSettings = channel.MotorDeviceSettings as ThorlabsBenchtopStepperMotorSettings;

            // Display info about device
            DeviceInfo deviceInfo = channel.GetDeviceInfo();
            Console.WriteLine("Device {0} = {1}", deviceInfo.SerialNumber, deviceInfo.Name);

            Home_Method1(channel);
            // or 
            //Home_Method2(channel);
            bool homed = channel.Status.IsHomed;

            // If a position is requested
            if (position != 0)
            {
                // Update velocity if required using real world methods
                if (velocity != 0)
                {
                    VelocityParameters velPars = channel.GetVelocityParams();
                    velPars.MaxVelocity = velocity;
                    channel.SetVelocityParams(velPars);
                }

                Move_Method1(channel, position);
                // or
                // Move_Method2(channel, position);

                Decimal newPos = channel.Position;
                Console.WriteLine("Device Moved to {0}", newPos);
            }

            channel.StopPolling();
            device.Disconnect(true);

            Console.ReadKey();
        }

        public static void Home_Method1(IGenericAdvancedMotor device)
        {
            try
            {
                Console.WriteLine("Homing device");
                device.Home(60000);
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to home device");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Device Homed");
        }

        public static void Move_Method1(IGenericAdvancedMotor device, decimal position)
        {
            try
            {
                Console.WriteLine("Moving Device to {0}", position);
                device.MoveTo(position, 60000);
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to move to position");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Device Moved");
        }



        public static void CommandCompleteFunction(ulong taskID)
        {
            if ((_taskID > 0) && (_taskID == taskID))
            {
                _taskComplete = true;
            }
        }

        public static void Home_Method2(IGenericAdvancedMotor device)
        {
            Console.WriteLine("Homing device");
            _taskComplete = false;
            _taskID = device.Home(CommandCompleteFunction);
            while (!_taskComplete)
            {
                Thread.Sleep(500);
                StatusBase status = device.Status;
                Console.WriteLine("Device Homing {0}", status.Position);

                // will need some timeout functionality;
            }
            Console.WriteLine("Device Homed");
        }

        public static void Move_Method2(IGenericAdvancedMotor device, decimal position)
        {
            Console.WriteLine("Moving Device to {0}", position);
            _taskComplete = false;
            _taskID = device.MoveTo(position, CommandCompleteFunction);
            while (!_taskComplete)
            {
                Thread.Sleep(500);
                StatusBase status = device.Status;
                Console.WriteLine("Device Moving {0}", status.Position);

                // will need some timeout functionality;
            }
            Console.WriteLine("Device Moved");
        }

    }
}
