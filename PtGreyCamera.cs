using System;
using System.Text;
using FlyCapture2Managed;

namespace ExperimentControl
{
    class PtGreyCamera

    {
        public static string GetBuildInfo()
        {
            FC2Version version = ManagedUtilities.libraryVersion;

            StringBuilder newStr = new StringBuilder();
            newStr.AppendFormat(
                "FlyCapture2 library version: {0}.{1}.{2}.{3}\n",
                version.major, version.minor, version.type, version.build);
            string newString = newStr.ToString();
            return newString;
        }
        public static string GetCameraInfo(CameraInfo camInfo)
        {
            StringBuilder newStr = new StringBuilder();
            newStr.Append("\n*** CAMERA INFORMATION ***\n");
            newStr.AppendFormat("Serial number - {0}\n", camInfo.serialNumber);
            newStr.AppendFormat("Camera model - {0}\n", camInfo.modelName);
            newStr.AppendFormat("Camera vendor - {0}\n", camInfo.vendorName);
            newStr.AppendFormat("Sensor - {0}\n", camInfo.sensorInfo);
            newStr.AppendFormat("Resolution - {0}\n", camInfo.sensorResolution);

            return newStr.ToString();
        }

        private static bool CheckSoftwareTriggerPresence(ManagedCamera cam)
        {
            const uint TriggerInquiry = 0x530;
            uint triggerInquiryValue = cam.ReadRegister(TriggerInquiry);

            if ((triggerInquiryValue & 0x10000) != 0x10000)
            {
                return false;
            }

            return true;
        }

        private static bool PollForTriggerReady(ManagedCamera cam)
        {
            const uint SoftwareTrigger = 0x62C;

            uint softwareTriggerValue = 0;

            do
            {
                softwareTriggerValue = cam.ReadRegister(SoftwareTrigger);
            }
            while ((softwareTriggerValue >> 31) != 0);

            return true;
        }

        private static bool FireSoftwareTrigger(ManagedCamera cam)
        {
            const uint SoftwareTrigger = 0x62C;
            const uint SoftwareTriggerFireValue = 0x80000000;

            cam.WriteRegister(SoftwareTrigger, SoftwareTriggerFireValue);

            return true;
        }

        public static void Test() //Name to be defined, structure to understand, transform message to exception, possibly dividing in different functions and even class
        {
            
            bool useSoftwareTrigger = true;

            ManagedBusManager busMgr = new ManagedBusManager();
            uint numCameras = busMgr.GetNumOfCameras();

            

            // Finish if there are no cameras
            if (numCameras == 0)
            {
                throw new NoCameraDetectedException();
            }

            ManagedPGRGuid guid = busMgr.GetCameraFromIndex(0); //If there is more than 1 camera, we take the first one

            ManagedCamera cam = new ManagedCamera();

            cam.Connect(guid);

            // Power on the camera
            const uint CameraPower = 0x610;
            const uint CameraPowerValue = 0x80000000;
            cam.WriteRegister(CameraPower, CameraPowerValue);

            const Int32 MillisecondsToSleep = 100;
            uint cameraPowerValueRead = 0;

            // Wait for camera to complete power-up
            do
            {
                System.Threading.Thread.Sleep(MillisecondsToSleep);

                cameraPowerValueRead = cam.ReadRegister(CameraPower);
            }
            while ((cameraPowerValueRead & CameraPowerValue) == 0);

            // Get the camera information
            CameraInfo camInfo = cam.GetCameraInfo();

            _ = GetCameraInfo(camInfo);

            if (!useSoftwareTrigger)
            {
                // Check for external trigger support
                TriggerModeInfo triggerModeInfo = cam.GetTriggerModeInfo();
                if (triggerModeInfo.present != true)
                {
                    throw new ExternalTriggerNotSupportedException();
                }
            }

            // Get current trigger settings
            TriggerMode triggerMode = cam.GetTriggerMode();

            // Set camera to trigger mode 0
            // A source of 7 means software trigger
            triggerMode.onOff = true;
            triggerMode.mode = 0;
            triggerMode.parameter = 0;

            if (useSoftwareTrigger)
            {
                // A source of 7 means software trigger
                triggerMode.source = 7;
            }
            else
            {
                // Triggering the camera externally using source 0.
                triggerMode.source = 0;
            }

            // Set the trigger mode
            cam.SetTriggerMode(triggerMode);


            // Get the camera configuration
            FC2Config config = cam.GetConfiguration();

            // Set the grab timeout to 5 seconds
            config.grabTimeout = 5000;

            // Set the camera configuration
            cam.SetConfiguration(config);

            #region Taking picture

            // Camera is ready, start capturing images
            cam.StartCapture();

            if (useSoftwareTrigger)
            {
                if (CheckSoftwareTriggerPresence(cam) == false) //Check if the camera support software trigger
                {
                    throw new SoftwareTriggerNotSupportedException();
                }
            }
            else
            {
                Console.WriteLine("Trigger the camera by sending a trigger pulse to GPIO%d.\n",
                  triggerMode.source);
            }


            
            ManagedImage rawImage = new ManagedImage();


            if (useSoftwareTrigger)
            {
                // Check that the trigger is ready
                bool retVal = PollForTriggerReady(cam);

                Console.WriteLine("Press the Enter key to initiate a software trigger.\n");
                Console.ReadLine();

                // Fire software trigger
                retVal = FireSoftwareTrigger(cam);
                if (retVal != true)
                {
                    Console.WriteLine("Error firing software trigger!");
                    Console.WriteLine("Press enter to exit...");
                    Console.ReadLine();
                    return;
                }
            }

            try
            {
                // Retrieve an image
                cam.RetrieveBuffer(rawImage);
            }
            catch (FC2Exception ex)
            {
                Console.WriteLine("Error retrieving buffer : {0}", ex.Message);
                
            }

            
            #endregion
            // Stop capturing images
            cam.StopCapture();

            // Turn off trigger mode
            triggerMode.onOff = false;
            cam.SetTriggerMode(triggerMode);

            // Disconnect the camera
            cam.Disconnect();
        }
    }
}
