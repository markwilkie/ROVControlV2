using System;

public class RawSensorMessage
{
    //Message type definition
    const byte messageType = 5;
    const byte messageLen = 1;

    //Define byte array for message itself
    private byte[] msgArray = new byte[messageLen];

    //Private member variables
    private int magX;
    private int magY;
    private int magZ;
    private int accX;
    private int accY;
    private int accZ;
    private int gyroRoll;
    private int gyroPitch;
    private int gyroYaw;

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

        //Get data from incoming message
        int msgNum = byteArray[0 + offset];

        magX = byteArray[1 + offset] | ((byteArray[2 + offset]) << 8);
        magY = byteArray[3 + offset] | ((byteArray[4 + offset]) << 8);
        magZ = byteArray[5 + offset] | ((byteArray[6 + offset]) << 8);

        accX = byteArray[7 + offset] | ((byteArray[8 + offset]) << 8);
        accY = byteArray[9 + offset] | ((byteArray[10 + offset]) << 8);
        accZ = byteArray[11 + offset] | ((byteArray[12 + offset]) << 8);

        gyroRoll = byteArray[13 + offset] | ((byteArray[14 + offset]) << 8);
        gyroPitch = byteArray[15 + offset] | ((byteArray[16 + offset]) << 8);
        gyroYaw = byteArray[17 + offset] | ((byteArray[18 + offset]) << 8); 
        
        return true;
    }

    public int GetMagX()
    {
        return magX;
    }
    public int GetMagY()
    {
        return magY;
    }
    public int GetMagZ()
    {
        return magZ;
    }
    public int GetAccX()
    {
        return accX;
    }
    public int GetAccY()
    {
        return accY;
    }
    public int GetAccZ()
    {
        return accZ;
    }
    public int GetGyroRoll()
    {
        return gyroRoll;
    }
    public int GetGyroPitch()
    {
        return gyroPitch;
    }
    public int GetGyroYaw()
    {
        return gyroYaw;
    }
}
