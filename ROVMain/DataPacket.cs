using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PacketData
{
    class DataPackets
    {
        //MESSAGE number contants
        public const int THRUSTER_MSG = 1;
        public const int SENS_MSG = 5;
        public const int ADC_MSG = 6;

        //List of data messages
        public ThrusterControlMessage thrusterControlMessage = new ThrusterControlMessage();
        public ADCMessage ADCMessage = new ADCMessage();
        public RawSensorMessage rawSensorMessage = new RawSensorMessage();

        //Remember where we left off on a partial packet
        int lastPos = 0;
        byte[] incomingMsg;

        /// <summary>
        /// Parse
        /// </summary>
        /// <param name="incomingMsg">Raw data off the wire</param>
        public bool ParsePacket(byte[] currentIncomingMsg)
        {
            // Parse data
            try
            {
                //Set total number of bytes of the message we're current working with
                int totalCurrentBytes = currentIncomingMsg.Length + lastPos;

                //Initialize array as needed
                if (lastPos == 0)
                {
                    incomingMsg = currentIncomingMsg;
                }
                else
                {
                    //Merge the new arrays
                    byte[] scratchArray = new byte[totalCurrentBytes];
                    //Array.Copy(incomingMsg, 0, scratchArray, 0, incomingMsg.Length);
                    //Array.Copy(currentIncomingMsg, 0, scratchArray, incomingMsg.Length, currentIncomingMsg.Length);
                    Array.Copy(incomingMsg, 0, scratchArray, 0, lastPos);
                    Array.Copy(currentIncomingMsg, 0, scratchArray, lastPos, currentIncomingMsg.Length); 
                    incomingMsg = scratchArray;
                }

                //Are we at least 2 bytes in??  If not, we know to wait some more...
                if (totalCurrentBytes < 2)
                {
                    lastPos = totalCurrentBytes;
                    return false;
                }

                //Get overall msg length and set msgIndex
                int fullMsgLen = incomingMsg[1];

                //Sanity check
                if (fullMsgLen > incomingMsg.Length || fullMsgLen < 2 || incomingMsg[0] != 1)
                {
                    lastPos = totalCurrentBytes;
                    return false;
                }

                //Has the full expected packet come in yet?  If not, go around again....
                if(totalCurrentBytes < fullMsgLen)
                {
                    lastPos = totalCurrentBytes;
                    return false;
                }

                //Since we've made it this far, we have a full packet
                lastPos = 0;

                //Check crc
                byte crcDigit = crc8(incomingMsg, fullMsgLen - 1); //don't include existing check digit when calcing
                if (crcDigit != incomingMsg[fullMsgLen - 1])
                {
                    MessageBox.Show("CRC MisMatch in Message Packet: " + incomingMsg[fullMsgLen - 1] + " " + crcDigit);
                    return false;
                }

                //Get msg number
                int msgNum = incomingMsg[2];

                //Parse based on message number
                if (msgNum == 1) //thrusters
                {
                    if (!thrusterControlMessage.ParseByteArray(incomingMsg,2) ) //Parse motors - returns false if problem
                    {
                        MessageBox.Show("Problem parsing Thurster Message: " + crcDigit);
                        return false;
                    }
                }
                if (msgNum == 5) //Raw Sensors
                {
                    if (!rawSensorMessage.ParseByteArray(incomingMsg, 2)) //Parse ADC - returns false if problem
                    {
                        MessageBox.Show("Problem parsing Raw Sensors Message: " + crcDigit);
                        return false;
                    }
                } 
                if (msgNum == 6) //ADC
                {
                    if (!ADCMessage.ParseByteArray(incomingMsg, 2)) //Parse ADC - returns false if problem
                    {
                        MessageBox.Show("Problem parsing ADC Message: " + crcDigit);
                        return false;
                    }
                }
            }
            catch (Exception parseException)
            {
                MessageBox.Show("Problem parsing data.\n\n" + parseException.StackTrace);
                return false;
            }

            //Return success
            return true;
        }

        //Reset things and start from the top
        public void ResetPacket()
        {
            lastPos = 0;
        }

        /// <summary>
        /// Build
        /// </summary>
        /// <param name="rawData">Build raw data stream for the wire</param>
        public byte[] BuildPacket(int messageToBuild)
        {
            int length = 0;
            byte[] outboundByteArray;

            //Get byte data for each message
            switch (messageToBuild)
            {
                case 1:
                    outboundByteArray = thrusterControlMessage.GetSendByteArray();  //Motors
                    break;
                case 5:
                    outboundByteArray = rawSensorMessage.GetSendByteArray();  //Raw sensor values
                    break;
                case 6:
                    outboundByteArray = ADCMessage.GetSendByteArray();  //ADC values
                    break;
                default:
                    MessageBox.Show("Packet Message Not Found: " + messageToBuild);
                    return null;
            }

            //Fix length up
            length = length + outboundByteArray.Length;

            //Build final data stream to send down the wire
            byte[] streamData = new byte[length + 4];
            streamData[0] = 1; //start byte
            streamData[1] = (byte)(length + 4); //length of this entire message
            Array.Copy(outboundByteArray, 0, streamData, 2, outboundByteArray.Length);  //copy in packet data
            streamData[length + 2] = 4; //end byte
            streamData[length + 3] = crc8(streamData,length+3); //check byte

            return streamData;
        }

        // calculate 8-bit CRC
        private byte crc8 (byte[] data, int len)
        {
          byte crc = 0;
          for(int i=0;i<len;i++)
            {
            byte inbyte = data[i];
            for (byte b=8; b>0 ; b--)
              {
              byte mix = (byte)((crc ^ inbyte) & 0x01);
              crc >>= 1;
              if (mix > 0) 
                crc ^= 0x8C;
              inbyte >>= 1;
              } 
            } 
          return crc;
        } 
    }
}

