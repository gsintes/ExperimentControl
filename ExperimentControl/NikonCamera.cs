using CameraControl.Devices;
using CameraControl.Devices.Classes;
using System.Threading;
using System;
using System.IO;
using NationalInstruments.DAQmx;


namespace ExperimentControl.ExperimentControl
{
    public class NikonCamera
    {
        #region Fields definition
        private readonly CameraDeviceManager deviceManager;
        private readonly string FolderForPhotos;
        private readonly DOTask triggerControl;
        private readonly DigitalSingleChannelWriter writer;
        private readonly RelayBoxComponent relayControl;
        #endregion

        #region Constructor
        /// <summary>
        /// Instantiate a new camera with the default or the current settings (not sure), avoid using this one
        /// </summary>
        /// <exception cref="NoCameraDetectedException">Thrown when their is no camera detected</exception>
        public NikonCamera()
        {
            triggerControl = new DOTask(System.Configuration.ConfigurationManager.AppSettings["NikonTrigger"], "NikonTrigger");
            writer = new DigitalSingleChannelWriter(triggerControl.Stream);

            relayControl = new RelayBoxComponent(System.Configuration.ConfigurationManager.AppSettings["NikonRelay"], "Nikon Power");
            PowerOn();
            

            deviceManager = new CameraDeviceManager();
            deviceManager.PhotoCaptured += DeviceManager_PhotoCaptured;
            deviceManager.CameraConnected += DeviceManager_CameraConnected;
            deviceManager.CameraDisconnected += DeviceManager_CameraDisconnected;
            FolderForPhotos = "FlowVisualization";
            bool ok=deviceManager.ConnectToCamera();
            int count = 0;
            while (!ok && count<10)
            {
                count++;
                Thread.Sleep(500);
                ok = deviceManager.ConnectToCamera();
            }
            if (!ok)
            {
                throw new NoCameraDetectedException { Source = "Nikon" };
            }
        
            deviceManager.SelectedCameraDevice.CaptureInSdRam = true;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Take a snap with the Nikon using the intervalometer
        /// </summary>
        public void Snap()
        {
            writer.WriteSingleSampleSingleLine(true, true);
            Thread.Sleep(100);
            writer.WriteSingleSampleSingleLine(true, false);
        }
      
        /// <summary>
        /// Called when a picture is captured with the Nikon, takes care of the saving
        /// </summary>
        /// <param name="o"></param>
        private void PhotoCaptured(object o)
        {
            if (!(o is PhotoCapturedEventArgs eventArgs))
            {
                DateTime date = DateTime.Now;
                string str = string.Format("{0}-{1:00}-{2:00},{3:00}:{4:00}: ERROR: Download photo from the camera: no argument given.",
                date.Year,
                date.Month,
                date.Day, date.Hour, date.Minute);
                using StreamWriter writerL = new StreamWriter("log.txt", true);
                writerL.WriteLine(str);
                return;
            }
            try
            {
                DateTime date = DateTime.Now;
                string fileName = Path.Combine(FolderForPhotos, String.Format("im_{0}-{1:00}-{2:00}_{3:00}h{4:00}.TIF", 
                    date.Year,
                    date.Month,
                    date.Day,
                    date.Hour,
                    date.Minute));
                fileName =
                        StaticHelper.GetUniqueFilename(
                            Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_", 0,
                            Path.GetExtension(fileName));
                // if file existtry to get a new filename to prevent file lost.
                // This is useful when camera is set to record in ram all filenames are thee same.
                if (File.Exists(fileName))
                {
                    fileName =
                        StaticHelper.GetUniqueFilename(
                            Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_", 0,
                            Path.GetExtension(fileName));
                }
                // check  if the folder existe if not create it
                if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                }
                eventArgs.CameraDevice.TransferFile(eventArgs.Handle, fileName);

                // the isBusy may be used internally, if file transfer is done, it should be set to false
                eventArgs.CameraDevice.IsBusy = false;
                

            }
            catch (Exception ex)
            {
                eventArgs.CameraDevice.IsBusy = false;
                DateTime date = DateTime.Now;
                string str = string.Format("{0}-{1:00}-{2:00},{3:00}:{4:00}: ERROR: Download photo from the camera: ",
                date.Year,
                date.Month,
                date.Day, date.Hour, date.Minute) + ex.Message;
                using StreamWriter writerLog = new StreamWriter("log.txt", true);
                writerLog.WriteLine(str);
            }
        }
        /// <summary>
        /// Turn on the Nikon by activating the relay on which it is plugged 
        /// </summary>
        public void PowerOn()
        {
            relayControl.TurnOn();
        }
        /// <summary>
        /// Turn off the Nikon by desactivating the relay on which it is plugged 
        /// </summary>
        public void PowerOff()
        {
            relayControl.TurnOff();
        }
        #endregion

        #region EventHandler
        private void DeviceManager_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
        {
            
            // to prevent freeze start the transfer process in a new thread
            Thread thread = new Thread(PhotoCaptured);
            thread.Start(eventArgs);
        }
        private void DeviceManager_CameraDisconnected(ICameraDevice cameraDevice)
        {
            DateTime date = DateTime.Now;
            string str = string.Format("{0}-{1:00}-{2:00},{3:00}:{4:00}: Nikon Camera disconnected",
            date.Year,
            date.Month,
            date.Day, date.Hour, date.Minute);
            using StreamWriter writerL = new StreamWriter("log.txt", true);
            writerL.WriteLine(str);
        }

        private void DeviceManager_CameraConnected(ICameraDevice cameraDevice)
        {
            bool ok = deviceManager.ConnectToCamera();
            if (!ok)
            {
                throw new NoCameraDetectedException();
            }
            else
            {
                DateTime date = DateTime.Now;
                string str = string.Format("{0}-{1:00}-{2:00},{3:00}:{4:00}: Nikon Camera connected",
                date.Year,
                date.Month,
                date.Day, date.Hour, date.Minute);
                using StreamWriter writerL = new StreamWriter("log.txt", true);
                writerL.WriteLine(str);
            }

        }
        #endregion

    }
}
