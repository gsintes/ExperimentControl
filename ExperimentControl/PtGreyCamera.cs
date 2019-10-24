using System;
using System.Text;
using FlyCapture2Managed;
using System.IO;

namespace ExperimentControl.ExperimentControl
{
    /// <summary>
    /// Enable the control of the point grey camera.
    /// </summary>
    public class PtGreyCamera
    {
        #region ATTRIBUTES DECLARATION  
        private readonly ManagedCamera cam;
        private PtGreyCameraSetting setting;
        #endregion

        #region CONSTRUCTORS

        /// <summary>
        ///Initialize a point Grey Camera, it take the first that it detect if their is more than one. 
        ///Give the default setting.
        /// </summary>
        /// <exception cref="NoCameraDetectedException">Thrown if no camera is detected.</exception>
        public PtGreyCamera()
        {
            using ManagedBusManager busMgr = new ManagedBusManager();
            setting = new PtGreyCameraSetting();
            uint numCameras = busMgr.GetNumOfCameras();

            if (numCameras == 0)
            {
                throw new NoCameraDetectedException { Source = "PointGrey" };
            }

            ManagedPGRGuid guid = busMgr.GetCameraFromIndex(0); //If there is more than 1 camera, we take the first one

            cam = new ManagedCamera();
            cam.Connect(guid);
            SetProp();
        }
        /// <summary>
        ///Initialize a point Grey Camera, it take the first that it detect if their is more than one. 
        ///Utilize the given settings to initialize the camera.
        /// </summary>
        /// <exception cref="NoCameraDetectedException">Thrown if no camera is detected.</exception>
        /// <param name="setting">Setting used for the camera</param>
        public PtGreyCamera(PtGreyCameraSetting setting)
        {
            using ManagedBusManager busMgr = new ManagedBusManager();
            this.setting = setting;
            uint numCameras = busMgr.GetNumOfCameras();
            Console.WriteLine(numCameras);
            // Finish if there are no cameras
            if (numCameras == 0)
            {
                throw new NoCameraDetectedException();
            }

            ManagedPGRGuid guid = busMgr.GetCameraFromIndex(0); //If there is more than 1 camera, we take the first one

            cam = new ManagedCamera();

            cam.Connect(guid);

            SetProp();
        }
        #endregion

        #region METHODS

        /// <summary>
        /// Get the information about the library used
        /// </summary>
        /// <returns>The library info</returns>
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
        /// <summary>
        /// Get the information about the camera.
        /// </summary>
        /// <returns>Camera info </returns>
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
        /// <summary>
        /// Check if the camera can be triggered by a software
        /// </summary>
        /// <returns>True if it can, false if not</returns>
        public bool CheckSoftwareTriggerPresence()
        {
            const uint TriggerInquiry = 0x530;
            uint triggerInquiryValue = cam.ReadRegister(TriggerInquiry);

            if ((triggerInquiryValue & 0x10000) != 0x10000)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Poll to checked if the trigger is ready
        /// </summary>
        /// <returns>True if ready, false if not</returns>
        private bool PollForTriggerReady()
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
        /// <summary>
        /// Trigger a picture and give a boolean confirming that it worked.
        /// </summary>
        /// <returns>True if the trigger worked, false if not.</returns>
        private bool FireSoftwareTrigger()
        {
            const uint SoftwareTrigger = 0x62C;
            const uint SoftwareTriggerFireValue = 0x80000000;

            cam.WriteRegister(SoftwareTrigger, SoftwareTriggerFireValue);

            return true;
        }

        ///<summary>
        ///Take a picture and save it in file name
        /// </summary>  
        /// <param name="fileName">File name where the picture is going to be saved> Must be a correct path</param>
        /// <exception cref="SoftwareTriggerNotSupportedException">Thrown when the camera doesn't support software triggering.</exception>
        /// <exception cref="ExternalTriggerNotSupportedException">Thrown when the camera doesn't support external triggering.</exception>
        /// <exception cref="TriggerFailedException">Thrown when the triggering had failed and the picture hasn't been taken.</exception>
        public void Snap(string fileName) // To thing of a possible separation to avoid setting  everything all the time
        {
            

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



            // Get current trigger settings
            TriggerMode triggerMode = cam.GetTriggerMode();

            // Set camera to trigger mode 0
            // A source of 7 means software trigger
            triggerMode.onOff = true;
            triggerMode.mode = 0;
            triggerMode.parameter = 0;

        
            // A source of 7 means software trigger
            triggerMode.source = 7;

         

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


            using (ManagedImage rawImage = new ManagedImage())
            {

                // Check that the trigger is ready
                bool retVal = PollForTriggerReady();

                // Fire software trigger
                bool retVal1 = FireSoftwareTrigger();
                if (!(retVal && retVal1))
                {
                    throw new TriggerFailedException();
                }
                
                try
                {
                    // Retrieve an image
                    cam.RetrieveBuffer(rawImage);
                   
                    rawImage.Save(fileName);
                    
                }
                catch (FC2Exception ex)
                {
                    Console.WriteLine("Error retrieving buffer : {0}", ex.Message);
                }

            }
            #endregion
            
            // Stop capturing images
            cam.StopCapture();

            // Turn off trigger mode
            triggerMode.onOff = false;
            cam.SetTriggerMode(triggerMode);
        }

        #region Set properties 
        public void ChangeSetting(PtGreyCameraSetting setting)
        {
            this.setting = setting;
            SetProp();
        }
        /// <summary>
        /// Set the current settings of the class in the camera
        /// </summary>
        private void SetProp()
        {
            SetShutter(setting.Shutter);
            SetFrameRate(setting.FrameRate);
            SetBrightness(setting.Brightness);
            SetGain(setting.Gain);
            SetAutoExposure();
        }
        ///<summary>
        ///Set the shutter time to the value shutter
        ///</summary>
        ///<param name="shutter">Duration of the shutter given in ms.</param>
        private void SetShutter(float shutter) //f at the end of the number to say we want a float and not a double
        {
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
        ///<summary>
        ///Set the frame rate to the value frm
        ///</summary>
        ///<param name="frm">Frame rate given in fps.</param>
        private void SetFrameRate(float frm)
        {
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
        ///<summary>
        ///Set the brightness.
        ///</summary>
        ///<param name="brightness">Brightness given in %.</param>
        private void SetBrightness(float brightness)
        {
            CameraProperty prop = new CameraProperty
            {
                type = PropertyType.Brightness,
                onOff = true,
                autoManualMode = false,
                absControl = true,
                absValue = brightness
            };
            cam.SetProperty(prop);

        }
        ///<summary>
        ///Set the gain time to the value gain
        ///</summary>
        ///<param name="gain">Gain set to the camera in dB.</param>
        ///
        private void SetGain(float gain)
        {
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
        ///<summary>
        ///Set the auto exposure.
        ///</summary>
        private void SetAutoExposure()
        {
            CameraProperty prop = new CameraProperty
            {
                type = PropertyType.AutoExposure,
                onOff = true,
                autoManualMode = false
            };//not sure how it is working but seems to be the default value
            cam.SetProperty(prop);

        }

        #endregion
        #endregion
    }
}