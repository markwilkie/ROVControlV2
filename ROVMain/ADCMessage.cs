using System;

public class ADCMessage
{
    //Message type definition
    const byte messageType = 6;
    const byte messageLen = 1;

    //Define byte array for message itself
    private byte[] msgArray = new byte[messageLen];

    //Private member variables
    private int adc0;
    private int adc1;
    private int adc2;
    private int adc3;
    private int adc4;
    private int adc5;
    private int adc6;
    private int adc7;
    private int adc8;
    private int adc9;
    private int adc10;
    private int adc11;
    private int adc12;
    private int adc13;
    private int adc14;
    private int adc15;

    //Get byte array to send down wire
    public byte[] GetSendByteArray()
    {
         msgArray[0] = (byte)messageType;

        return msgArray;
    }

    //Parse byte array
    public bool ParseByteArray(byte[] byteArray,int offset)
    {
        //Make sure not an error message
        if (byteArray[1] < 18)
            return true;

        //First, get data from incoming message
        int msgNum = byteArray[0 + offset];

        adc0 = byteArray[1 + offset];
        adc0 = adc0 | ((byteArray[5 + offset] & 3) << 8);

        adc1 = byteArray[2 + offset];
        adc1 = adc1 | ((byteArray[5 + offset] & 12) << 6);

        adc2 = byteArray[3 + offset];
        adc2 = adc2 | ((byteArray[5 + offset] & 48) << 4);

        adc3 = byteArray[4 + offset];
        adc3 = adc3 | ((byteArray[5 + offset] & 192) << 2);

        adc4 = byteArray[6 + offset];
        adc4 = adc4 | ((byteArray[10 + offset] & 3) << 8);

        adc5 = byteArray[7 + offset];
        adc5 = adc5 | ((byteArray[10 + offset] & 12) << 6);

        adc6 = byteArray[8 + offset];
        adc6 = adc6 | ((byteArray[10 + offset] & 48) << 4);

        adc7 = byteArray[9 + offset];
        adc7 = adc7 | ((byteArray[10 + offset] & 192) << 2);

        adc8 = byteArray[11 + offset];
        adc8 = adc8 | ((byteArray[15 + offset] & 3) << 8);

        adc9 = byteArray[12 + offset];
        adc9 = adc9 | ((byteArray[15 + offset] & 12) << 6);

        adc10 = byteArray[13 + offset];
        adc10 = adc10 | ((byteArray[15 + offset] & 48) << 4);

        adc11 = byteArray[14 + offset];
        adc11 = adc11 | ((byteArray[15 + offset] & 192) << 2);

        adc12 = byteArray[16 + offset];
        adc12 = adc12 | ((byteArray[20 + offset] & 3) << 8);

        adc13 = byteArray[17 + offset];
        adc13 = adc13 | ((byteArray[20 + offset] & 12) << 6);

        adc14 = byteArray[18 + offset];
        adc14 = adc14 | ((byteArray[20 + offset] & 48) << 4);

        adc15 = byteArray[19 + offset];
        adc15 = adc15 | ((byteArray[20 + offset] & 192) << 2);

        return true;
    }

    public int GetADC(int adcNum)
    {
        switch (adcNum)
        {
            case 0:
                return adc0;
            case 1:
                return adc1;
            case 2:
                return adc2;
            case 3:
                return adc3;
            case 4:
                return adc4;
            case 5:
                return adc5;
            case 6:
                return adc6;
            case 7:
                return adc7;
            case 8:
                return adc8;
            case 9:
                return adc9;
            case 10:
                return adc10;
            case 11:
                return adc11;
            case 12:
                return adc12;
            case 13:
                return adc13;
            case 14:
                return adc14;
            case 16:
                return adc15;
            default:
                return 9;
        }
    }
}
