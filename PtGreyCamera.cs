using System;
using System.Text;
using FlyCapture2Managed;

namespace ExperimentControl
{
    class PtGreyCamera

    {
        private ManagedCamera cam;
        
        public PtGreyCamera()
        {

            

            ManagedBusManager busMgr = new ManagedBusManager();
            uint numCameras = busMgr.GetNumOfCameras();



            // Finish if there are no cameras
            if (numCameras == 0)
            {
                throw new NoCameraDetectedException();
            }

            ManagedPGRGuid guid = busMgr.GetCameraFromIndex(0); //If there is more than 1 camera, we take the first one

            cam = new ManagedCamera();

            cam.Connect(guid);
            busMgr.Dispose();
        }
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
        public string GetCameraInfo()
        {
            CameraInfo camInfo = cam.GetCameraInfo();
            StringBuilder newStr = new StringBuilder();
            newStr.Append("\n*** CAMERA INFORMATION ***\n");
            newStr.AppendFormat("Serial number - {0}\n", camInfo.serialNumber);
            newStr.AppendFormat("Camera model - {0}\n", camInfo.modelName);
            newStr.AppendFormat("Camera vendor - {0}\n", camInfo.vendorName);
            newStr.AppendFormat("Sensor - {0}\n", camInfo.sensorInfo);
            newStr.AppendFormat("Resolution - {0}\n", camInfo.sensorResolution);

            return newStr.ToString();
        }

        private bool CheckSoftwareTriggerPresence()
        {
            const uint TriggerInquiry = 0x530;
            uint triggerInquiryValue = cam.ReadRegister(TriggerInquiry);

            if ((triggerInquiryValue & 0x10000) != 0x10000)
            {
                return false;
            }

            return true;
        }

        private bool PollForTriggerReady(ManagedCamera cam)
        {
            const uint SoftwareTrigger = 0x62C;

            uint softwareTriggerValue;
            do
            {
                softwareTriggerValue = cam.ReadRegister(SoftwareTrigger);
            }
            while ((softwareTriggerValue >> 31) != 0);

            return true;
        }

        private bool FireSoftwareTrigger()
        {
            const uint SoftwareTrigger = 0x62C;
            const uint SoftwareTriggerFireValue = 0x80000000;

            cam.WriteRegister(SoftwareTrigger, SoftwareTriggerFireValue);

            return true;
        }
         

        public void Snap() // To thing of a possible separation to avoid setting  everything all the time
        {


            bool useSoftwareTrigger = true;

            // Power on the camera
            const uint CameraPower = 0x610;
            const uint CameraPowerValue = 0x80000000;
            cam.WriteRegister(CameraPower, CameraPowerValue);

            const Int32 MillisecondsToSleep = 100;
            uint cameraPowerValueRead;

            // Wait for camera to complete power-up
            do
            {
                System.Threading.Thread.Sleep(MillisecondsToSleep);

                cameraPowerValueRead = cam.ReadRegister(CameraPower);
            }
            while ((cameraPowerValueRead & CameraPowerValue) == 0);

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
                if (CheckSoftwareTriggerPresence() == false) //Check if the camera support software trigger
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

                // Fire software trigger
                bool retVal1 = FireSoftwareTrigger();
                if (!(retVal && retVal1))
                {
                    throw new TriggerFailedException();
                }
            }

            try
            {
                // Retrieve an image
                cam.RetrieveBuffer(rawImage);
                rawImage.Save("C:/Users/gs656local/Documents/Test/test.bmp");
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
        }

        #region Set properties 
        public void SetShutter(float shutter)
        {
            ///<summary>
            ///Set the shutter time to the value shutter
            ///</summary>
            ///<param name="shutter">Duration of the shutter given in ms.</param>
            CameraProperty prop = new CameraProperty
            {
                type = PropertyType.Shutter,
                onOff = true,
                autoManualMode = false,
                absControl = true,
                absValue = shutter
            };
            cam.SetProperty(prop);

        }

        public void SetFrameRate(float frm=35)
        {
            ///<summary>
            ///Set the shutter time to the value shutter
            ///</summary>
            ///<param name="frm">Frame rate given in fps. Optional, default 35fps.</param>
            CameraProperty prop = new CameraProperty
            {
                type = PropertyType.FrameRate,
                onOff = true,
                autoManualMode = false,
                absControl = true,
                absValue = frm
            };
            cam.SetProperty(prop);

        }

        public void SetGain(float gain=0)
        {
            ///<summary>
            ///Set the gain time to the value gain
            ///</summary>
            ///<param name="gain">Gain set to the camera in dB. Optional, default value=0</param>
            ///
            CameraProperty prop = new CameraProperty
            {
                type = PropertyType.Gain,
                onOff = true,
                autoManualMode = false,
                absControl = true,
                absValue = gain
            };
            cam.SetProperty(prop);

        }
        #endregion
    }
}
