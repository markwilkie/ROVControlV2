using System;   
using System.IO;
using System.IO.Ports; 
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
using HexDump;

namespace SerialCom
{
    class ComPortClass
    {
        private ROVControl.frmMain mainForm;
        private SerialPort serialPort;
        RadioButton rxRadioControl;
        RadioButton txRadioControl;
        private byte[] receivedBytes;
        private double ComTimeOut;
        private double averageTime = 0; 
        Stopwatch stopWatch;

        //Com control vars
        Boolean timeoutFlag = false;
        Boolean readytoSend = true;
        int timeouts = 0;
        
        //Constructor
        public ComPortClass(ROVControl.frmMain mainFrm)
        {
            mainForm = mainFrm;

            //Find tx radio control
            foreach (Control c in mainForm.Controls) //assuming this is a Form
            {
                if (c.Name == "radioButtonTx")
                {
                    txRadioControl = (RadioButton)c; //found
                }
            }

            //Find rx radio control
            foreach (Control c in mainForm.Controls) //assuming this is a Form
            {
                if (c.Name == "radioButtonRx")
                    rxRadioControl = (RadioButton)c; //found
            }
        }

        //init Com port
        public void Init(string comPort, int comBaud,int comTimeout)
        {
            //Get Stopwatch all squared away
            ComTimeOut = comTimeout;
            stopWatch = new Stopwatch();

            // Open the ports for communications
            try
            {
                serialPort = new SerialPort(comPort, comBaud, Parity.None, 8, StopBits.One);
                serialPort.Handshake = Handshake.None;
                serialPort.WriteTimeout = 500;

                // Finally, open it
                serialPort.Open(); 
            }
            catch (Exception portexcep)
            {
                throw new ArgumentException(portexcep.ToString());
            }
        }

        //close things up
        public void Close()
        {
            // Close the port
            serialPort.Close();
        }

        //Read Com port (non blocking)
        public Boolean ReadComData(PacketData.DataPackets dataPackets)
        {
            //double ms1 = Math.Round(((stopWatch.ElapsedTicks * 1000.0) / Stopwatch.Frequency), 0);
            //while (serialPort.BytesToRead == 0) ;
            //double ms2 = Math.Round(((stopWatch.ElapsedTicks * 1000.0) / Stopwatch.Frequency), 0);

            //Check is bytes are available
            if (serialPort.BytesToRead > 0)
            {
                //Update radio button to say that we've received serial data
                mainForm.Invoke(new EventHandler(delegate
                {
                    rxRadioControl.Checked = true;
                }));

                //Read port
                receivedBytes = new byte[serialPort.BytesToRead];
                int recv = serialPort.Read(receivedBytes, 0, receivedBytes.Length);
    
                //Parse resulting byte array
                if (dataPackets.ParsePacket(receivedBytes))  // if true, we're done - if not, partial - just go around again.
                {
                    //stop timer
                    stopWatch.Stop();

                    //Now ready to send again, and reset timeout flag
                    readytoSend = true;
                    timeoutFlag = false;
                }

                //Update radio button to say that we've received serial data
                mainForm.Invoke(new EventHandler(delegate
                {
                    rxRadioControl.Checked = false;
                }));
            }

            //Keep running average of elapsed milli-seconds
            //  The "smoothing factor" is '.1' which means it's a relatively slow moving average
            averageTime = ((double)stopWatch.ElapsedMilliseconds * .05) + (averageTime * (1 - .05));
            mainForm.UpdateStopWatchLabel(((long)averageTime).ToString());

            //Check if timeout
            if (averageTime > ComTimeOut)
            {
                timeouts++;
                mainForm.UpdateTimeOuts(timeouts.ToString());
                timeoutFlag = true; //set flag
                readytoSend = true;  //Go ahead and send now again
                dataPackets.ResetPacket(); //reset packet and start from the stop
            }

            //return ready to send
            return readytoSend;
        }

        //send buffer out port
        public Boolean SendBuffer(PacketData.DataPackets dataPackets,int messageToSend)
        {
            //Only send if we're ready to.  This keeps things from backing up....
            if (readytoSend)
            {
                //See how long this is taking
                stopWatch.Restart();

                //check radio control
                ////////////////////txRadioControl.Checked = true;


                //Now that we've sent, flip flag
                readytoSend = false;

                //send data packets
                byte[] packet = dataPackets.BuildPacket(messageToSend);
                serialPort.Write(packet, 0, packet.Length);

                //Uncheck radio control
                txRadioControl.Checked = false;

                //Debug
                System.Diagnostics.Debug.WriteLine("UART Sending...");
                System.Diagnostics.Debug.WriteLine(Utils.HexDump(packet));

                //We did send
                return true;
            }
            //We did not send
            return false;
        }
    }//class
}//namespace
