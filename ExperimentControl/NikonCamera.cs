using CameraControl.Devices;
using CameraControl.Devices.Classes;
using System.Threading;
using System;
using System.IO;
using NationalInstruments.DAQmx;


namespace ExperimentControl
{
    class NikonCamera
    {
        private readonly CameraDeviceManager deviceManager;
        private readonly string FolderForPhotos;
        private readonly DOTask control;
        private readonly DigitalSingleChannelWriter writer;

        #region Constructor
        /// <summary>
        /// Instantiate a new camera with the default or the current settings (not sure), avoid using this one
        /// </summary>
        /// <exception cref="NoCameraDetectedException">Thrown when their is no camera detected</exception>
        public NikonCamera()
        {
            control = new DOTask("Dev1/port0/line4", "cam");
            writer = new DigitalSingleChannelWriter(control.Stream);
            
            deviceManager = new CameraDeviceManager();
            deviceManager.PhotoCaptured += DeviceManager_PhotoCaptured;
            FolderForPhotos = "FlowVisualization";
            bool ok=deviceManager.ConnectToCamera();
            if (!ok)
            {
                throw new NoCameraDetectedException();
            }
        
            deviceManager.SelectedCameraDevice.CaptureInSdRam = true;
        }
        #endregion 
        public void Snap()
        {
            writer.WriteSingleSampleSingleLine(true, true);
            Thread.Sleep(100);
            writer.WriteSingleSampleSingleLine(true, false);
        }
      
        private void PhotoCaptured(object o)
        {
            PhotoCapturedEventArgs eventArgs = o as PhotoCapturedEventArgs;
            if (eventArgs == null)
            {
                return;
            }
            try
            {
                DateTime date = DateTime.Now;
                string fileName = Path.Combine(FolderForPhotos, String.Format("im_{0}-{1}-{2},{3}.TIF",date.Year,
                date.Month,
                date.Day,
                date.Hour));
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
                string str = string.Format("{0}-{1}-{2},{3}:{4}: ERROR: Download photo from the camera: ",
                date.Year,
                date.Month,
                date.Day, date.Hour, date.Minute) + ex.Message;
                using (StreamWriter writer = new StreamWriter("log.txt", true))
                {
                    writer.WriteLine(str);
                }
            }
        }
        private void DeviceManager_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
        {
            
            // to prevent freeze start the transfer process in a new thread
            Thread thread = new Thread(PhotoCaptured);
            thread.Start(eventArgs);
        }


    }
}
