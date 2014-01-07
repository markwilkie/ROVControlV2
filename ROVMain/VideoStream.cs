using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.VFW;

namespace Video
{
    class VideoStream
    {
        private bool DeviceExist = false;
        private bool DeviceStreaming = false;
        private bool DeviceRecording = false;
        private string codec;
        private string filename;
        private PictureBox videoBox;
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource = null;
        private AVIWriter writer = null;

        //Constructor
        public VideoStream(PictureBox pictureBox,string code,string name)
        {
            videoBox = pictureBox;
            codec = code;
            filename = name;
        }

        // get the devices name
        public void getCamList(ComboBox comboBox)
        {
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                comboBox.Items.Clear();
                if (videoDevices.Count == 0)
                    throw new ApplicationException();

                DeviceExist = true;
                foreach (FilterInfo device in videoDevices)
                {
                    comboBox.Items.Add(device.Name);
                }
                comboBox.SelectedIndex = 0; //make dafault to first cam
            }
            catch (ApplicationException)
            {
                DeviceExist = false;
                comboBox.Items.Add("No capture device on your system");
            }
        }

        //display video properties page
        public void DisplayProperties(IntPtr hWnd)
        {
            videoSource.DisplayPropertyPage(hWnd);
        }

        //toggle start and stop video streaming.  Returns true if streaming.
        public bool StartStopVideo(int deviceIndex)
        {
            //If the device is not streaming, start it
            if (!DeviceStreaming)
            {
                if (DeviceExist)
                {
                    videoSource = new VideoCaptureDevice(videoDevices[deviceIndex].MonikerString);
                    videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                    videoSource.DesiredFrameSize = new Size(160, 140);
                    //videoSource.DesiredFrameRate = 10;

                    CloseVideoSource(); 
                    videoSource.Start();
                    DeviceStreaming = true;
                }
            }
            else
            {
                if (videoSource.IsRunning)
                {
                    CloseVideoSource();
                    DeviceStreaming = false;
                }
            }

            return DeviceStreaming;
        }

        //eventhandler if new frame is ready
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap img = (Bitmap)eventArgs.Frame.Clone();

            //do processing here
            videoBox.Image = img;

            // add the image as a new frame of video file
            if(DeviceRecording)
                writer.AddFrame(img);
        }

        //Start and stop record stream
        public bool StartStopRecord()
        {
            if (!DeviceRecording)
            {
                // instantiate  writer, use WMV codec ("wmv3" should work, otherwise using "DIB ")
                DeviceRecording = true;
                writer = new AVIWriter(codec);
                writer.Open(filename + "_" + DateTime.Now.Ticks + ".avi", 160, 120);
            }
            else
            {
                DeviceRecording = false;
                writer.Close();
            }

            return DeviceRecording;
        }
        
        //close the device safely
        public void CloseVideoSource()
        {
            //close source
            if (!(videoSource == null))
                if (videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    videoSource = null;
                }

            //close record stream
            if(DeviceRecording)
                writer.Close();
        }
    }
}
