using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;


namespace ROVControl
{
    public partial class frmMain : Form
    {
        //Declare joystick extents constants
        private const int JOYSTICKMAX = 65535;
        private const int JOYSTICKMIN = 0;

        //Joystick and controller objects
        private CraftController craftController; //declare craft controller instance var
        private JoystickInterface.Joystick jst;  //declare joystick instance var
        private KeyboardInterface.Keyboard keyb; //declare keyboard instance var

        //Compass
        private CompassControl compassControl;

        //Graph
        private GraphControl graphControl;

        //Video stream object
        private Video.VideoStream videoStream;

        //Object to manage control loop
        ControlLoop controlLoop;

        public frmMain()
        {
            InitializeComponent();

            //Init compass control
            compassControl = new CompassControl(compass);

            //init graph
            graphControl = new GraphControl(this, display);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //Let's create control loop, but not start it yet
            controlLoop = new ControlLoop();

            //Let's get a craft controller instance
            try
            {
                craftController = new CraftController(this, JOYSTICKMAX);
            }
            catch (Exception craftException)
            {
                MessageBox.Show("Problem creating a craft controller.\n\n" + craftException.Message);
                Application.Exit();
            }

            //get joystick squared away
            try
            {
                // grab the joystick
                jst = new JoystickInterface.Joystick(this.Handle, Properties.Settings.Default.JOYSTICK_THRESHOLD);

                //Make sure we found a stick
                if (jst.FindJoysticks() == 0)
                {
                    MessageBox.Show("No Joystick Found - using keyboard / mouse");
                    //tmrUpdateStick.Enabled = false;
                }
                else
                {
                    //tmrUpdateStick.Enabled = true;
                    //Grab the first joystick out of the list
                    jst.AcquireJoystick();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Problem finding USB Joystick.\n\n" + exception.Message);
                Application.Exit();
            }

            //get keyboard squared away
            try
            {
                // grab the keyboard
                keyb = new KeyboardInterface.Keyboard(this.Handle);
                keyb.InitKeyboard();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Problem attaching to keyboard.\n\n" + exception.Message);
                Application.Exit();
            }

            //init video capture devices
            this.videoBoxControl.Paint += new System.Windows.Forms.PaintEventHandler(this.videoBoxControl_Paint);
            videoStream = new Video.VideoStream(videoBoxControl, Properties.Settings.Default.CODEC, Properties.Settings.Default.VIDEOPATH);

            //Init controls
            InitControls();
        }

        //Temp timer to update graph
        private void GraphTimer1_Tick(object sender, EventArgs e)
        {
            graphControl.GraphTimer_Tick();
        }

        //Painter for video OSD
        private void videoBoxControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Brush b = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, Color.White));
            Font f = new System.Drawing.Font("Tahoma", 12);
            e.Graphics.DrawString("Hello World", f, b, 0, 0);
            e.Graphics.DrawString("Hello World222", f, b, 50, 50);
        } 

        // Start the infinite loop to read input and control ROV
        private void buttonStart_Click(object sender, EventArgs e)
        {
            //Start loop
            if (!controlLoop.running && controlLoop.stopSignal)
            {                
                //Start main loop  (never returns from start, but does from a stop)
                buttonStart.Text = "Online"; 
                controlLoop.Start(this);
            }
            else
            {
                controlLoop.Stop();
                buttonStart.Text = "Offline";
            }
        }

        public void UpdateStopWatchLabel(string time)
        {
            labelStopWatch.Text = time;
        }

        public void UpdateTimeOuts(string timeouts)
        {
            labelTimeOuts.Text = timeouts;
        }

        //Read upd (used by control loop)
        public void SendReceiveSerial_Tick()
        {
            craftController.SendReceiveSerial();
        }

        //Update controller with craft next commands - then update feedback on thursters
        public void MoveCraft_Tick()
        {
            //Now tell craft controller to move forward, reverse, left, and right
            craftController.MoveCraft();

            //Update camera state
            craftController.PanCamera();
            craftController.TiltCamera();

            //Update with controller feedback
            UpdateThursterFeedback();
        }
        

        //Update joystick state
        public void UpdateStick_Tick()
        {
            //Just return if no joystick is enabled
            if (!jst.enabled)
                return;

            // get latest status from joystick
            jst.UpdateStatus();

            //if no update, just return
            //if (!jst.joystickUpdated)
            //    return;

            //Update each exis with current joystick value
            craftController.Xaxis.SetCurrentPosition(jst.axisX1);
            craftController.Yaxis.SetCurrentPosition(jst.axisY1);
            craftController.Zaxis.SetCurrentPosition(jst.axisZ);
            craftController.Raxis.SetCurrentPosition(jst.axisR);

            //Now smooth these axis out to help get rid of "jitter" from joystick
            craftController.Xaxis.SmoothInput();
            craftController.Yaxis.SmoothInput();
            craftController.Zaxis.SmoothInput();
            craftController.Raxis.SmoothInput();

            //Update the craft conroller with current joystick coord
            UpdateXYAxis();
            UpdateZAxis();

            //Update Camera stuff
            UpdatePOV(jst.povPos);

            /*
            // update each button status
            foreach (Control btn in flpButtons.Controls)
            {
                if (btn is ROVControl.Button)
                {
                    ((ROVControl.Button)btn).ButtonStatus =
                        jst.Buttons[((ROVControl.Button)btn).ButtonId - 1];
                }
            }
             */
        }

        //Update keyboard state
        public void UpdateKeyboard_Tick()
        {
            // get latest status from keyboard
            keyb.UpdateStatus();
        }

        //Update XY Axis form labels
        private void UpdateXYAxis()
        {
            //Calc "throttle" percentage for Y axis and update label appropriately
            int yPos = craftController.Yaxis.GetLatestPosition();
            string yThrottle = calcPercentage(yPos, true);
            trackBarForwardReverse.Value = (JOYSTICKMAX - yPos);

            //Calc "throttle" percentage for X axis and update label appropriately
            int xPos = craftController.Xaxis.GetLatestPosition();
            string xThrottle = calcPercentage(xPos, true);
            trackBarLeftRight.Value = xPos;

            //Calc "throttle" percentage for swivel and update label appropriately
            int swivelPos = craftController.Raxis.GetLatestPosition();
            string throttle = calcPercentage(swivelPos, false);
            trackBarSwivel.Value = (swivelPos);
        }

        //Update Z Axis
        private void UpdateZAxis()
        {
            //Get current z axis position
            int pos = craftController.Zaxis.GetLatestPosition();

            //Calc "throttle" percentage and update label appropriately
            string throttle = calcPercentage(pos,true);

            //Update window control appropriately
            trackBarUpDown.Value = (JOYSTICKMAX - pos);
        }
        
        //Update POV  (called from joystick timer)
        private void UpdatePOV(int pos)
        {
            //Interpret inputs
            bool panLeft = false;
            bool panRight = false;
            bool tiltUp = false;
            bool tiltDown = false;

            //based on return value, set sliders and activate servoes
            if (pos == 0)
                tiltUp = true;
            if (pos == 9000)
                panRight = true;
            if (pos == 18000)
                tiltDown = true;
            if (pos == 27000)
                panLeft = true;

            //Update radio buttons
            radioButtonCamUp.Checked = tiltUp;
            radioButtonCamDown.Checked = tiltDown;
            radioButtonCamLeft.Checked = panLeft;
            radioButtonCamRight.Checked = panRight;

            //Update Camera
            UpdateCamera(panLeft, panRight, tiltUp, tiltDown);
        }

        //Update Camera pan/tilt
        private void UpdateCamera(bool panLeft, bool panRight, bool tiltUp, bool tiltDown)
        {
            //Update state in craft controller
            craftController.camPanLeft = panLeft;
            craftController.camPanRight = panRight;
            craftController.camTiltUp = tiltUp;
            craftController.camTiltDown = tiltDown;
        }

        //Move camera if radio button is checked by user
        private void radioButtonCamUp_Click(object sender, EventArgs e)
        {
            //set flags
            bool panLeft = false;
            bool panRight = false;
            bool tiltUp = true;
            bool tiltDown = false;

            //Update camera
            UpdateCamera(panLeft, panRight, tiltUp, tiltDown);

            //turn off check box now that we're done
            radioButtonCamUp.Checked = false;
        }

        private void radioButtonCamDown_Click(object sender, EventArgs e)
        {
            //set flags
            bool panLeft = false;
            bool panRight = false;
            bool tiltUp = false;
            bool tiltDown = true;

            //Update camera
            UpdateCamera(panLeft, panRight, tiltUp, tiltDown);

            //turn off check box now that we're done
            radioButtonCamDown.Checked = false;
        }

        private void radioButtonCamLeft_Click(object sender, EventArgs e)
        {
            //set flags
            bool panLeft = true;
            bool panRight = false;
            bool tiltUp = false;
            bool tiltDown = false;

            //Update camera
            UpdateCamera(panLeft, panRight, tiltUp, tiltDown);

            //turn off check box now that we're done
            radioButtonCamLeft.Checked = false;
        }

        private void radioButtonCamRight_Click(object sender, EventArgs e)
        {
            //set flags
            bool panLeft = false;
            bool panRight = true;
            bool tiltUp = false;
            bool tiltDown = false;

            //Update camera
            UpdateCamera(panLeft, panRight, tiltUp, tiltDown);

            //turn off check box now that we're done
            radioButtonCamRight.Checked = false;
        }

        // calc percentage based on joystick input
        private String calcPercentage(int joystickInput,bool showNeg)
        {
            int center = (JOYSTICKMAX - JOYSTICKMIN) / 2;
            int perc = 0;

            //Figure "throttle"
            perc = (int)(((float)(center - joystickInput) / center) * 100);
            if (!showNeg && perc < 0)
                 perc = perc * -1;

            //Now, construct the appropriate string
            String returnText=null;
            if (perc != 0)
                returnText = perc + "%";
            else 
                returnText = "Stopped";

            return returnText;
        }

        //Update progress bars which represent thrusters
        private void UpdateThursterFeedback()
        {
            lblX.Text = craftController.Xaxis.GetLatestPositionPercentage().ToString();
            lblY.Text = craftController.Yaxis.GetLatestPositionPercentage().ToString();
            lblR.Text = craftController.Raxis.GetLatestPositionPercentage().ToString();
            lblZ.Text = craftController.Zaxis.GetLatestPositionPercentage().ToString();


            lbl_Heading.Text = craftController.GetDataPackets().thrusterControlMessage.GetTargetHeading().ToString();
            lblMag.Text = craftController.GetDataPackets().thrusterControlMessage.GetTargetMagnitude().ToString();


            lblMot1.Text = craftController.GetDataPackets().thrusterControlMessage.GetMot45().ToString();
            lblMot2.Text = craftController.GetDataPackets().thrusterControlMessage.GetMot135().ToString();
            lblMot3.Text = craftController.GetDataPackets().thrusterControlMessage.GetMot225().ToString();
            lblMot4.Text = craftController.GetDataPackets().thrusterControlMessage.GetMot315().ToString();
            lblMotVert.Text = craftController.GetDataPackets().thrusterControlMessage.GetMotVert().ToString();

            //Update vector control
            int mag=craftController.GetDataPackets().thrusterControlMessage.GetTargetMagnitude();
            if (mag > 0 && (craftController.Xaxis.GetLatestPositionPercentage() != 0 || craftController.Yaxis.GetLatestPositionPercentage() != 0))
            {
                int vector = craftController.GetDataPackets().thrusterControlMessage.GetTargetHeading();
                thurstVectorCircleControl.PrimaryMarkerAngle = (360 - vector) + 90;
                thurstVectorCircleControl.PrimaryMarkerBorderColor = Color.Black;
                thurstVectorCircleControl.PrimaryMarkerSolidColor = Color.Red;
            }
            else
            {
                thurstVectorCircleControl.PrimaryMarkerBorderColor = Color.Transparent;
                thurstVectorCircleControl.PrimaryMarkerSolidColor = Color.Transparent;
            }

            //Update motor gif based on vector
            int motSpeed = craftController.GetDataPackets().thrusterControlMessage.GetMot45();
            if(motSpeed > 90)
                pictureBoxRF.Image = Properties.Resources.ROV_RF_For;
            else if(motSpeed < 90)
                pictureBoxRF.Image = Properties.Resources.ROV_RF_Rev;
            else
                pictureBoxRF.Image = Properties.Resources.ROV_RF_Stop;

            motSpeed = craftController.GetDataPackets().thrusterControlMessage.GetMot135();
            if (motSpeed > 90)
                pictureBoxRR.Image = Properties.Resources.ROV_RR_For;
            else if (motSpeed < 90)
                pictureBoxRR.Image = Properties.Resources.ROV_RR_Rev;
            else
                pictureBoxRR.Image = Properties.Resources.ROV_RR_Stop;

            motSpeed = craftController.GetDataPackets().thrusterControlMessage.GetMot225();
            if (motSpeed > 90)
                pictureBoxLR.Image = Properties.Resources.ROV_LR_For;
            else if (motSpeed < 90)
                pictureBoxLR.Image = Properties.Resources.ROV_LR_Rev;
            else
                pictureBoxLR.Image = Properties.Resources.ROV_LR_Stop;

            motSpeed = craftController.GetDataPackets().thrusterControlMessage.GetMot315();
            if (motSpeed > 90)
                pictureBoxLF.Image = Properties.Resources.ROV_LF_For;
            else if (motSpeed < 90)
                pictureBoxLF.Image = Properties.Resources.ROV_LF_Rev;
            else
                pictureBoxLF.Image = Properties.Resources.ROV_LF_Stop;

            motSpeed = craftController.GetDataPackets().thrusterControlMessage.GetMotVert();
            if (motSpeed > 90)
                pictureBoxCenter.Image = Properties.Resources.ROV_Center_Up;
            else if (motSpeed < 90)
                pictureBoxCenter.Image = Properties.Resources.ROV_Center_Down;
            else
                pictureBoxCenter.Image = Properties.Resources.ROV_Center_Stop;

            //Update motor feedback
            int feedback = craftController.GetDataPackets().thrusterControlMessage.GetMot45();
            progressBarPosMot45.Value = feedback > 90 ? feedback = feedback - 90 : 90 - feedback;
            feedback = craftController.GetDataPackets().thrusterControlMessage.GetMot135();
            progressBarPosMot135.Value = feedback > 90 ? feedback = feedback - 90 : 90 - feedback;
            feedback = craftController.GetDataPackets().thrusterControlMessage.GetMot225();
            progressBarPosMot225.Value = feedback > 90 ? feedback = feedback - 90 : 90 - feedback;
            feedback = craftController.GetDataPackets().thrusterControlMessage.GetMot315();
            progressBarPosMot315.Value = feedback > 90 ? feedback = feedback - 90 : 90 - feedback;
            feedback = craftController.GetDataPackets().thrusterControlMessage.GetMotVert();
            progressBarPosMotVert.Value = feedback > 90 ? feedback = feedback - 90 : 90 - feedback;

            /*
            //Update actual angle label
            labelLeftPos.Text = craftController.LeftThruster.GetCurrentAngle().ToString();
            labelRightPos.Text = craftController.RightThruster.GetCurrentAngle().ToString();
            labelCenterPos.Text = craftController.CenterThruster.GetCurrentAngle().ToString();

            //Update left thruster feedback
            lblLeftPos.Text = craftController.LeftThruster.GetCurrentPosition().ToString();
            int currentPos = craftController.LeftThruster.GetCurrentPosition();
            int centerPos = ((craftController.LeftThruster.GetMaxPos() - craftController.LeftThruster.GetMinPos()) / 2) + craftController.LeftThruster.GetMinPos();
            if (currentPos < centerPos)
            {
                progressBarNegLeft.Value = 0;
                progressBarPosLeft.Value = (centerPos - currentPos);
            }
            else
            {
                progressBarPosLeft.Value = 0;
                progressBarNegLeft.Value = (currentPos - centerPos);
            }

            //Update right thruster feedback
            lblRightPos.Text = craftController.RightThruster.GetCurrentPosition().ToString();
            currentPos = craftController.RightThruster.GetCurrentPosition();
            centerPos = ((craftController.RightThruster.GetMaxPos() - craftController.RightThruster.GetMinPos()) / 2) + craftController.RightThruster.GetMinPos();
            if (currentPos < centerPos)
            {
                progressBarNegRight.Value = 0;
                progressBarPosRight.Value = (centerPos - currentPos);
            }
            else
            {
                progressBarNegRight.Value = (currentPos - centerPos);
                progressBarPosRight.Value = 0;
            }

            //Update center thruster feedback
            lblCenterPos.Text = craftController.CenterThruster.GetCurrentPosition().ToString();
            currentPos = craftController.CenterThruster.GetCurrentPosition();
            centerPos = ((craftController.CenterThruster.GetMaxPos() - craftController.CenterThruster.GetMinPos()) / 2) + craftController.CenterThruster.GetMinPos();
            if (currentPos < centerPos)
            {
                progressBarNegCenter.Value = 0;
                progressBarPosCenter.Value = (centerPos - currentPos);
            }
            else
            {
                progressBarPosCenter.Value = 0;
                progressBarNegCenter.Value = (currentPos - centerPos);
            }
            */
        }

        //Update things even if user grabs scroll bar instead of using joystick
        private void trackBarForwardReverse_Scroll(object sender, EventArgs e)
        {
            //Reverse calc joystick position based on control position
            craftController.Yaxis.SetCurrentPosition(JOYSTICKMAX - trackBarForwardReverse.Value);

            UpdateXYAxis();
        }

        // Update leak status
        private void tmrUpdateSensorStatus_Tick(object sender, EventArgs e)
        {
            UpdateSensorStatus_Tick();
        }

        // Update sensor status
        public void UpdateSensorStatus_Tick()
        {
            //Raw sensor data
            lblMagX.Text = craftController.GetDataPackets().rawSensorMessage.GetMagX().ToString();
            lblMagY.Text = craftController.GetDataPackets().rawSensorMessage.GetMagY().ToString();
            lblMagZ.Text = craftController.GetDataPackets().rawSensorMessage.GetMagZ().ToString();

            lblAccX.Text = craftController.GetDataPackets().rawSensorMessage.GetAccX().ToString();
            lblAccY.Text = craftController.GetDataPackets().rawSensorMessage.GetAccY().ToString();
            lblAccZ.Text = craftController.GetDataPackets().rawSensorMessage.GetAccZ().ToString();

            lblGyroRoll.Text = craftController.GetDataPackets().rawSensorMessage.GetGyroRoll().ToString();
            lblGyroPitch.Text = craftController.GetDataPackets().rawSensorMessage.GetGyroPitch().ToString();
            lblGyroYaw.Text = craftController.GetDataPackets().rawSensorMessage.GetGyroYaw().ToString();

            try
            {
                /*
                 * //Get data packets received from ROV from com object 
                PacketData.DataPackets dataPackets = craftController.GetUDPCom().getPacketData();

                //Get leak data from these packets
                DisplayLeakSensorData(radioButton1, "Electroncs: ", dataPackets.leakSensorParser);
                DisplayLeakSensorData(radioButton2, "Camera: ", dataPackets.leakSensorParser.GetSensorData(1));
                DisplayLeakSensorData(radioButton3, "Left Thruster: ", dataPackets.leakSensorParser.GetSensorData(2));
                DisplayLeakSensorData(radioButton4, "Right Thruster: ", dataPackets.leakSensorParser.GetSensorData(3));
                DisplayLeakSensorData(radioButton5, "Center Thruster: ", dataPackets.leakSensorParser.GetSensorData(4));

                //Get voltage data from these packets
                DisplayVoltageCurrentSensorData(0, panel2, dataPackets.voltageSensorParser.GetSensorData(1), dataPackets.currentSensorParser.GetSensorData(2));
                DisplayVoltageCurrentSensorData(2, panel2, dataPackets.voltageSensorParser.GetSensorData(0), dataPackets.currentSensorParser.GetSensorData(1));
                DisplayVoltageCurrentSensorData(1, panel2, dataPackets.voltageSensorParser.GetSensorData(2), dataPackets.currentSensorParser.GetSensorData(0));
                 * */
            }
            catch (Exception comException)
            {
                MessageBox.Show("Problem receiving sensor data.\n\n" + comException.StackTrace);
                Application.Exit();
            }
        }

        /*
        //display leak sensor data correctly
        private void DisplayLeakSensorData(RadioButton radioButton, string label, PacketData.LeakSensorParser leakSensorParser)
        {
            bool leakPresent = false;
            string sensorDataString = "";
            int[] sensorDataArray = leakSensorParser.GetElectronicsSensorData();
            int leakThreshold = Properties.Settings.Default.LEAK_SENSOR_THRESHOLD;

            //Loop through leak sensors to see if we've got one
            for (int i = 0; i < sensorDataArray.Length; i++)
            {
                //See if leak
                if (sensorDataArray[i] > leakThreshold)
                    leakPresent = true;

                //Build label
                sensorDataString = sensorDataString + sensorDataArray[i];
                if(i<(sensorDataArray.Length-1))
                    sensorDataString = sensorDataString + ",";
            }

            //Now build final label and update control
            UpdateLeakControl(radioButton, label, sensorDataString, leakPresent);
        }

        private void DisplayLeakSensorData(RadioButton radioButton, string label, string leakData)
        {
            DisplayLeakSensorData(radioButton, label, System.Convert.ToInt32(leakData));
        }

        private void DisplayLeakSensorData(RadioButton radioButton, string label, int leakData)
        {
            //leak??
            if (leakData > Properties.Settings.Default.LEAK_SENSOR_THRESHOLD)
                UpdateLeakControl(radioButton, label, leakData.ToString(), true);
            else
                UpdateLeakControl(radioButton, label, leakData.ToString(), false);
        }

        private void UpdateLeakControl(RadioButton radioButton, string label, string leakData,bool leakPresent)
        {
            if (leakPresent)
            {
                radioButton.Checked = true;
                radioButton.ForeColor = Color.Red;
                radioButton.Text = label + "LEAK!! " + leakData;
            }
            else
            {
                radioButton.Checked = false;
                radioButton.ForeColor = Color.Black;
                radioButton.Text = label + leakData;
            }
        }

        //display voltage/current sensor data correctly
        private void DisplayVoltageCurrentSensorData(int motorNumber, Panel panel2, string voltageData, string currentData)
        {
            //Determine which label based on motorNumber
            string vLabelName = "labelV" + motorNumber.ToString();
            string cLabelName = "labelC" + motorNumber.ToString();

            //Get & set volatage label control
            Control[] controlList = panel2.Controls.Find(vLabelName, false);
            Label voltageLabel = (Label)controlList[0];
            voltageLabel.Text = voltageData;

            //Get & set current label control
            controlList = panel2.Controls.Find(cLabelName, false);
            Label currentLabel = (Label)controlList[0];
            currentLabel.Text = currentData;

            //Get & set voltage meter control
            controlList = panel2.Controls.Find("levelsMeterVoltage", false);  
            Teroid.LevelsMeter.LevelsMeter voltageMeter = (Teroid.LevelsMeter.LevelsMeter)controlList[0];
            float voltageAmount = (float)Convert.ToDouble(voltageData);

            //Check extents
            if (voltageAmount < 0)
                voltageAmount = 0;
            if (voltageAmount > 14)
                voltageAmount = 14;

            //Now, set voltage
            voltageMeter[motorNumber] = (float)Math.Round((voltageAmount-.5),0);

            //Get & set current meter control
            controlList = panel2.Controls.Find("levelsMeterCurrent", false);
            Teroid.LevelsMeter.LevelsMeter currentMeter = (Teroid.LevelsMeter.LevelsMeter)controlList[0];
            float currentAmount = (float)Convert.ToDouble(currentData);
            if (currentAmount < 0)
                currentAmount = 0;
            if (currentAmount > 15)
                currentAmount = 15;
            currentMeter[motorNumber] = currentAmount;
        }
         * */

        private void trackBarLeftRight_Scroll(object sender, EventArgs e)
        {
            //Reverse calc joystick position based on control position
            craftController.Xaxis.SetCurrentPosition(trackBarLeftRight.Value);

            UpdateXYAxis();
        }

        private void trackBarSwivel_Scroll(object sender, EventArgs e)
        {
            //Reverse calc joystick position based on control position
            craftController.Raxis.SetCurrentPosition(trackBarSwivel.Value);

            UpdateXYAxis();
        }

        private void trackBarUpDown_Scroll(object sender, EventArgs e)
        {
            //Reverse calc joystick position based on control position
            int currentPos = JOYSTICKMAX - trackBarUpDown.Value;
            craftController.Zaxis.SetCurrentPosition(currentPos);

            UpdateZAxis();
        }

        private void InitControls()
        {
            //Get list of video streams
            videoStream.getCamList(comboBoxVideoSources);
            
            //Update xy and z
            UpdateXYAxis();
            UpdateZAxis();
        }

        //Start/Stop video based on selected source
        private void btnVideoStartStop_Click(object sender, EventArgs e)
        {
            bool videoStreaming = videoStream.StartStopVideo(comboBoxVideoSources.SelectedIndex);

            if (videoStreaming)
                lblCamStatus.Text = "Camera is Streaming";
            else
                lblCamStatus.Text = "Camera is Stopped";
        }

        //Display video setting
        private void btnVideoSettings_Click(object sender, EventArgs e)
        {
            videoStream.DisplayProperties(this.Handle);
        }

        //Clean up on form close
        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(videoStream != null)
                videoStream.CloseVideoSource();
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            bool videoRecording = videoStream.StartStopRecord();

            if (videoRecording)
                btnRecord.BackColor = Color.Red;
            else
                btnRecord.BackColor = Color.White;
        }
    }
}