using CameraControl.Devices;
using CameraControl.Devices.Classes;
using System.Threading;
using System;
using System.IO;

namespace ExperimentControl
{
    class NikonCamera
    {
        private readonly CameraDeviceManager deviceManager;
        private readonly string FolderForPhotos;

        #region Constructor
        /// <summary>
        /// Instantiate a new camera with the default or the current settings (not sure), avoid using this one
        /// </summary>
        /// <exception cref="NoCameraDetectedException">Thrown when their is no camera detected</exception>
        public NikonCamera()
        {
            deviceManager = new CameraDeviceManager();
            deviceManager.PhotoCaptured += DeviceManager_PhotoCaptured;
            FolderForPhotos = "FlowVisualization";
            bool ok=deviceManager.ConnectToCamera();
            if (!ok)
            {
                throw new NoCameraDetectedException();
            }
        
            deviceManager.SelectedCameraDevice.CaptureInSdRam = true;
            //SetProp("640", "1/80");
        }
        #endregion 
        public void Snap()
        {
            bool retry = false;
            int count = 0;
            do
            {
                try
                {
                    deviceManager.SelectedCameraDevice.CapturePhotoNoAf();
                    
                    
                }
                catch (DeviceException ex)
                {
                    if ((ex.ErrorCode == ErrorCodes.MTP_Device_Busy ||
                        ex.ErrorCode == ErrorCodes.ERROR_BUSY))
                    {
                        Thread.Sleep(100);
                        retry = true;
                        count++;
                    }
                }
            } while (retry && count < 15);
            Console.WriteLine(count);
            if (count == 15)
            {
                throw new TriggerFailedException();
            }
            
        }
        public void SetProp(string ISO, string shutter)
        {
            deviceManager.SelectedCameraDevice.IsoNumber.Value = ISO;
            deviceManager.SelectedCameraDevice.ShutterSpeed.Value = shutter;

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
