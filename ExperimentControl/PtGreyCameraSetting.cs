using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ExperimentControl
{
    class PtGreyCameraSetting
    {
        #region Properties declaration
        private readonly float shutter;
        private readonly float frameRate;
        private readonly float gain;
        private readonly float brightness;
        #endregion

        #region Assesors
        public float FrameRate => frameRate;

        public float Shutter => shutter;

        public float Gain => gain;

        public float Brightness => brightness;
        #endregion

        #region Constructors
        public PtGreyCameraSetting(float shutter = 0.511f, float frameRate = 35, float gain = 0, float brightness = 0)
        {
            this.shutter = shutter;
            this.frameRate = frameRate;
            this.gain = gain;
            this.brightness = brightness;
        }
        /// <summary>
        /// Build the camera setting written in the file.
        /// If some info are missing, they are put to their default value.
        /// </summary>
        /// <param name="file">File where the camera settings are written</param>
        /// <exception cref="FormatNotRespectedException">Thrown when the file doesn't respect the expected format</exception>
        /// <exception cref="IOException">Thrown when there is a problem in reading the file</exception>
        public PtGreyCameraSetting(string file)
        {
            bool shutterDone = false;
            bool frDone = false;
            bool gainDone = false;
            bool brightDone = false;
            string line;
            
            using (StreamReader reader = new StreamReader(file))
            {
                int i = 0;
                while((line = reader.ReadLine()) != null)
                {
                    i++;
                    if (line.Contains("Shutter="))
                    {
                        this.shutter = GetValue(line);
                        shutterDone = true;
                    }
                    else if (line.Contains("Gain="))
                    {
                        this.gain = GetValue(line);
                        gainDone = true;
                    }
                    else if (line.Contains("FrameRate="))
                    {
                        this.frameRate = GetValue(line);
                        frDone = true;
                    }
                    else if (line.Contains("Brightness="))
                    {
                        this.brightness = GetValue(line);
                        brightDone = true;
                    }
                    
                }
            }
            if(!shutterDone)
            {
                this.shutter = 0.511f;
            }
            if(!frDone)
            {
                this.frameRate = 35;
            }
            if(!gainDone)
            {
                this.gain = 0;
            }
            if(!brightDone)
            {
                this.brightness = 0;
            }

        }
        #endregion

        #region Methods
        public override string ToString()
        {
            string str = "Current setting:\n Shutter = " + Shutter + " ms\n FrameRate= "
                + FrameRate + " fps\n Gain=" + Gain + " dB\n Brightness = " + Brightness + " %\n";
            return str;
        }
        /// <summary>
        /// Return the value from a string like "name = value (eventually unit)")
        /// </summary>
        /// <param name="line">Should be like "name = value (eventually unit)"</param>
        /// <returns>Return the value.</returns>
        /// <exception cref="FormatNotRespectedException"> Thrown when the line doesn't respect the expectexd format</exception>
        static public float GetValue(string line)
        {
            float value = -1;
            string pattern = @"(\d*\.\d{0,3})";
            foreach (Match m in Regex.Matches(line, pattern))
            {
                value = float.Parse(m.Groups[1].Value);
                
            }
            if(value != -1)
            {
                return value;
            }
            else
            {
                throw new FormatNotRespectedException();
            }
                
        }
        #endregion
    }
}
