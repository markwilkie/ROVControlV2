using System;
using System.Collections.Generic;


namespace ROVControl
{
    class ArduinoTestHarness
    {
        //Take message zero, and send simulated response.  (in this case, motor assignments based on curve)
        public byte[] GetResponseToMsgZero(byte[] msgZero)
        {
            //First, get data from incoming message
            int startByte = msgZero[0] >> 3;
            int msgNum = msgZero[0] & 7;
            int heading = msgZero[1];
            int magnitude = msgZero[2] >> 1;
            int magDirection = msgZero[2] & 1;
            int rotation = msgZero[3] >> 1;
            int rotDirection = msgZero[3] & 1;
            int ascent = msgZero[4] >> 1;
            int ascDirection = msgZero[4] & 1;
            int checkDigit = msgZero[5];

            //Determine secondary motor
            int secMot;
            if (heading > 0 && heading <= 90)
                secMot = 45;
            else if (heading > 90 && heading <= 180)
                secMot = 135;
            else if (heading > 180 && heading <= 270)
                secMot = 225;
            else
                secMot = 315;

            //Determine primary motor
            int priMot;
            if (heading > secMot)
                priMot = secMot + 90;
            else
                priMot = secMot - 90;
            if (priMot > 360)
                priMot = priMot - 360;
            else if (priMot < 0)
                priMot = priMot + 360;

            //Convert heading for use with magnitude (we're only calculating for 45 degrees)
            double convHeading = Math.Abs(secMot - heading);

            //Convert converted heading to force
            double c1 = -100;
            double c2 = -0.0174532925202266;
            double c3 = -182.212373908208;
            double motMagnitude = c1 * Math.Tan(c2 * convHeading + c3);

            //Now, figure what each motor should be at
            int mot45, mot135, mot225, mot315;
            if (priMot == 45 || priMot == 225)
            {
                mot45 = 100;
                mot135 = (int)motMagnitude;
            }
            else
            {
                mot45 = (int)motMagnitude;
                mot135 = 100;
            }
            if (mot45 >= 45 && mot45 <= 225)
                mot45 = mot45 * -1;
            if (mot135 >= 135 && mot135 <= 315)
                mot135 = mot135 * -1;
            mot225 = mot45; 
            mot315 = mot135;

            //Return values
        }
    }
}
