using CameraControl.Devices;

namespace ExperimentControl
{
    class NikonCamera
    {
        CameraDeviceManager deviceManager;

        #region Constructor
        /// <summary>
        /// Instantiate a new camera with the default or the current settings (not sure), avoid using this one
        /// </summary>
        /// <exception cref="NoCameraDetectedException">Thrown when their is no camera detected</exception>
        public NikonCamera()
        {
            deviceManager = new CameraDeviceManager();
            bool ok=deviceManager.ConnectToCamera();
            if (!ok)
            {
                throw new NoCameraDetectedException();
            }
        }
        #endregion 
        public void Snap(string filename)
        {
            deviceManager.SelectedCameraDevice.CapturePhotoNoAf();
            //deviceManager.LastCapturedImage.
        }
        public void SetProp(string ISO, string shutter)
        {
            deviceManager.SelectedCameraDevice.IsoNumber.Value = ISO;
            deviceManager.SelectedCameraDevice.ShutterSpeed.Value = shutter;

        }
    }
}
