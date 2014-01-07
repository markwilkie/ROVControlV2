using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ROVControl
{

    //Container class for servos, ESC's, serial object for data packets, and the serial object for craft communications
    class CraftController
    {
        const int STOPPED_VALUE = 1200;

        //Instance of form so we can update controls
        private frmMain mainForm;

        //declare object for the serial port used to Tx and Rx
        private SerialCom.ComPortClass serialCom;
        private PacketData.DataPackets dataPackets;

        //Declare lists used for serial message pump
        private int[] normalMsgs = {PacketData.DataPackets.THRUSTER_MSG};
        private int[] debugMsgs = {   PacketData.DataPackets.THRUSTER_MSG, 
                                      PacketData.DataPackets.SENS_MSG, 
                                      PacketData.DataPackets.THRUSTER_MSG, 
                                      PacketData.DataPackets.ADC_MSG };
        private int[] messagePumpArray;
        private int currentPumpIndex = 0;

        //Declare variables for camera pan/tilt increments
        private int panRightIncrement;
        private int panLeftIncrement;
        private int tiltUpIncrement;
        private int tiltDownIncrement;

        //Declare variable for camera pan/tilt state
        public bool camTiltUp = false;
        public bool camTiltDown = false;
        public bool camPanLeft = false;
        public bool camPanRight = false;

        //Create axis objects for each axis to track current values/state from joystick
        public Axis Xaxis = null;
        public Axis Yaxis = null;
        public Axis Raxis = null;
        public Axis Zaxis = null;

        //constructor with joystick max (65K)
        public CraftController(frmMain mainFrm,int axisMax)
        {
            //get pointer to form
            mainForm = mainFrm;

            ////create an instance of the TCP port used to Tx and Rx
            serialCom = new SerialCom.ComPortClass(mainForm);
            dataPackets = new PacketData.DataPackets();

            //Init message pump
            //messagePumpArray = normalMsgs;
            messagePumpArray = debugMsgs;

            //Grab values for camera pan and tilt
            panRightIncrement = Properties.Settings.Default.PAN_RIGHT_INCREMENT;
            panLeftIncrement = Properties.Settings.Default.PAN_LEFT_INCREMENT;
            tiltUpIncrement = Properties.Settings.Default.TILT_UP_INCREMENT;
            tiltDownIncrement = Properties.Settings.Default.TILT_DOWN_INCREMENT;

            //declare axis objects
            Xaxis = new Axis(axisMax);
            Yaxis = new Axis(axisMax,true);
            Raxis = new Axis(axisMax);
            Zaxis = new Axis(axisMax,true);

            //get data from the wire squared away
            try
            {
                serialCom.Init(Properties.Settings.Default.SERIALPORT, Properties.Settings.Default.SERIALBAUD, Properties.Settings.Default.SERIALTIMEOUT);  //init port
            }
            catch (Exception comException)
            {
                MessageBox.Show("Problem opening Com Port " + Properties.Settings.Default.SERIALPORT + " \n\n" + comException.Message);
                //Application.Exit();
            }
        }

        //Send and receive from Serial Port
        //
        //Returns true if successfully read
        //
        public void SendReceiveSerial()
        {
            //get message pump sorted
            if (currentPumpIndex >= messagePumpArray.Length)
                currentPumpIndex = 0;

            //Build and send current message
            if (serialCom.SendBuffer(dataPackets, messagePumpArray[currentPumpIndex]))
                currentPumpIndex++;  //only increment if sucessfully sent

            //Read response from port
            serialCom.ReadComData(dataPackets);
        }
        
        public PacketData.DataPackets GetDataPackets()
        {
            return dataPackets;
        }

        //Move craft
        public void MoveCraft()
        {
            //Get data packets received from ROV from com object 
            ThrusterControlMessage thrusterControlMessage = dataPackets.thrusterControlMessage;

            //Determine target heading and magnitude from x/y coordinates
            double radAngle = Math.Atan2(Xaxis.GetLatestPositionPercentage(), Yaxis.GetLatestPositionPercentage());
            int targetHeading = (int)(radAngle * 180 / Math.PI);
            targetHeading = targetHeading <= 0 ? targetHeading += 360 : targetHeading;

            int absRotation = Raxis.GetLatestPositionAbsPercentage();
            int targetMagnitude = (int)(Math.Sqrt(Math.Pow(Yaxis.GetLatestPositionAbsPercentage(), 2) + Math.Pow(Xaxis.GetLatestPositionAbsPercentage(), 2)));
            targetMagnitude = targetMagnitude < absRotation ? absRotation : targetMagnitude;

            //Set datapacket
            thrusterControlMessage.SetTargetHeading(targetHeading);
            thrusterControlMessage.SetTargetMagnitude(targetMagnitude);
            thrusterControlMessage.SetTargetRotation(Raxis.GetLatestPositionPercentage());
            thrusterControlMessage.SetTargetAscent(Zaxis.GetLatestPositionPercentage());
        }

        //Pan camera
        public void PanCamera()
        {
            /*
            if (camPanLeft)
                CameraPan.IncrementControllerWithAngle(panLeftIncrement, 0, true);
            else if (camPanRight)
                CameraPan.IncrementControllerWithAngle(panRightIncrement, 0, true);
            else
                CameraPan.CenterController();
             * */
         }

        //Tilt camera
        public void TiltCamera()
        {
            /*
            if (camTiltUp)
                CameraTilt.IncrementControllerWithAngle(tiltUpIncrement,0,true);
            else if (camTiltDown)
                CameraTilt.IncrementControllerWithAngle(tiltDownIncrement,0,true);
            else
                CameraTilt.IncrementControllerWithAngle(0, 0, true);
             * */
        }
    }
      
}
