using System;   
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace IPCom
{
    class IPPortClass
    {
        private ROVControl.frmMain mainForm;
        RadioButton rxRadioControl;
        RadioButton txRadioControl;
        //IPEndPoint serverEndPoint;
        //Socket serverSocket;
        //EndPoint remoteEndPoint;
        TcpClient server;
        NetworkStream ns;

        private byte[] receivedBytes;
        Stopwatch stopWatch;
        Queue timerQueue;

        //IP control vars
        double IPTimeOut;
        Boolean timeoutFlag = false;
        Boolean readytoSend = true;
        int timeouts = 0;
        
        //Constructor
        public IPPortClass(ROVControl.frmMain mainFrm)
        {
            mainForm = mainFrm;
            timerQueue = new Queue();

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

        //init UDP port
        public void Init(string IPAddr, int IPPort, int timeout)
        {
            //Get Stopwatch all squared away
            IPTimeOut = (double)timeout;
            stopWatch = new Stopwatch();

            // Open the ports for communications
            try
            {
                //serverEndPoint = new IPEndPoint(IPAddress.Parse(IPAddr), IPPort);
                //serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //remoteEndPoint = (EndPoint) new IPEndPoint(IPAddress.Any, 0);

                server = new TcpClient(IPAddr, IPPort);
                ns = server.GetStream();
            }
            catch (SocketException portexcep)
            {
                throw new ArgumentException(portexcep.ToString());
            }
        }

        //close things up
        public void Close()
        {
            // Close the port
            //serverSocket.Close();
            server.Close();
        }

        //Read UDP port (non blocking)
        public void ReadUdpData(PacketData.DataPackets dataPackets)
        {
            double ms5 = Math.Round(((stopWatch.ElapsedTicks * 1000.0) / Stopwatch.Frequency), 0);

            //while (serverSocket.Available == 0)
            while(server.Available==0)
            {
                System.Threading.Thread.Sleep(50);
            }

            double ms6 = Math.Round(((stopWatch.ElapsedTicks * 1000.0) / Stopwatch.Frequency), 0);

            //Check is bytes are available
            //if (serverSocket.Available > 0)
            if(server.Available>=0)
            {
                //Update radio button to say that we've received serial data
                mainForm.Invoke(new EventHandler(delegate
                {
                    rxRadioControl.Checked = true;
                }));

                double ms7 = Math.Round(((stopWatch.ElapsedTicks * 1000.0) / Stopwatch.Frequency), 0);

                //Read port
                receivedBytes = new byte[1024];
                //int recv = serverSocket.ReceiveFrom(receivedBytes, ref remoteEndPoint);
                ns = server.GetStream();
                ns.Read(receivedBytes, 0, receivedBytes.Length);

                double ms8 = Math.Round(((stopWatch.ElapsedTicks * 1000.0) / Stopwatch.Frequency), 0);


                //Parse resulting byte array
                dataPackets.ParsePacket(receivedBytes);

                double ms9 = Math.Round(((stopWatch.ElapsedTicks * 1000.0) / Stopwatch.Frequency), 0);


                //Now ready to send again, and reset timeout flag
                readytoSend = true;
                timeoutFlag = false;

                //Update radio button to say that we've received serial data
                mainForm.Invoke(new EventHandler(delegate
                {
                    rxRadioControl.Checked = false;
                }));

                double ms10 = Math.Round(((stopWatch.ElapsedTicks * 1000.0) / Stopwatch.Frequency), 0);


                //Stop watch
                stopWatch.Stop();
            }

            //Update stopwatch
            double ms = Math.Round(((stopWatch.ElapsedTicks * 1000.0) / Stopwatch.Frequency), 0);
            if (timerQueue.Count >= 10)
                timerQueue.Dequeue();  //always keep the queue at 10
            timerQueue.Enqueue(ms);

            //Get average
            double totalTime=0;
            foreach (double time in timerQueue)
                totalTime = totalTime + time;
            double averageTime=totalTime/timerQueue.Count;
            mainForm.UpdateStopWatchLabel(averageTime.ToString());

            //Check if timeout
            if (averageTime > IPTimeOut)
            {
                timeouts++;
                mainForm.UpdateTimeOuts(timeouts.ToString());
                timeoutFlag = true; //set flag
                readytoSend = true;  //Go ahead and send now again
            }
        }

        //send buffer out port
        public void SendBuffer(PacketData.DataPackets dataPackets)
        {
            //Only send if we're ready to.  This keeps things from backing up....
            if (readytoSend)
            {
                //See how long this is taking
                stopWatch.Restart();

                //check radio control
                txRadioControl.Checked = true;

                //Now that we've sent, flip flag
                readytoSend = false;

                //send data packets
                byte[] packet = dataPackets.BuildPacket();

                double ms1 = Math.Round(((stopWatch.ElapsedTicks * 1000.0) / Stopwatch.Frequency), 0);

                //int bytesSent = serverSocket.SendTo(packet, packet.Length, SocketFlags.None, serverEndPoint);
                ns.Write(packet, 0, packet.Length);
                ns.Flush();

                double ms2 = Math.Round(((stopWatch.ElapsedTicks * 1000.0) / Stopwatch.Frequency), 0);

                //Uncheck radio control
                txRadioControl.Checked = false;

                double ms3 = Math.Round(((stopWatch.ElapsedTicks * 1000.0) / Stopwatch.Frequency), 0);

                //Read udp
                ReadUdpData(dataPackets);

                double ms4 = Math.Round(((stopWatch.ElapsedTicks * 1000.0) / Stopwatch.Frequency), 0);
            }
        }
    }//class
}//namespace
