using System;

public class ThrusterControlMessage
{
    //Message type definition
    const byte messageType = 1;
    const byte messageLen = 5;

    //Define byte array for message itself
    private byte[] msgArray = new byte[messageLen];

    //Private member variables
    private int targetHeading;
    private int targetMagnetude;
    private int targetRotation;
    private int targetRotationDir; 
    private int targetAscent;
    private int targetAscentDir;

    private int mot45;
    private int mot135;
    private int mot225;
    private int mot315;
    private int motVert;


    //Set target heading if you already know your exact heading
    public void SetTargetHeading(int tHeading)
    {
        targetHeading = tHeading;
    }

    public int GetTargetHeading()
    {
        return targetHeading;
    }

    //Set target magnitude (throttle)
    public void SetTargetMagnitude(int tMagnetude)
    {
        targetMagnetude = Math.Abs(tMagnetude);
        if (targetMagnetude > 100) targetMagnetude = 100;
    }

    public int GetTargetMagnitude()
    {
        return targetMagnetude;
    }

    //Set target rotation
    public void SetTargetRotation(int tRotation)
    {
        targetRotationDir = tRotation < 0 ? 0 : 1;
        targetRotation = Math.Abs(tRotation);
    }

    public int GetTargetAbsRoation()
    {
        return targetRotation;
    }

    //Get rotation direction
    private int GetTargetRotationDirection()
    {
        return targetRotationDir;
    }

    //Set target rotation
    public void SetTargetAscent(int tAscent)
    {
        targetAscentDir = tAscent < 0 ? 0 : 1;
        targetAscent = Math.Abs(tAscent);
    }

    public int GetTargetAbsAscent()
    {
        return targetAscent;
    }

    //Get rotation direction
    private int GetTargetAscentDirection()
    {
        return targetAscentDir;
    }

    //Get byte array to send down wire
    public byte[] GetSendByteArray()
    {
        msgArray[0] = (byte)messageType;
        msgArray[1] = (byte)targetHeading;
        msgArray[2] = (byte)((targetMagnetude << 1) | (targetHeading >> 8));
        msgArray[3] = (byte)(Math.Abs(targetRotation) << 1 | (GetTargetRotationDirection() & 1));
        msgArray[4] = (byte)(Math.Abs(targetAscent) << 1 | (GetTargetAscentDirection() & 1));

        return msgArray;
    }

    //Parse byte array
    public bool ParseByteArray(byte[] byteArray,int offset)
    {
        //Make sure not an error message
        if (byteArray[1] == 8)
            return true;

        //First, get data from incoming message
        int msgNum = byteArray[0 + offset];
        mot45 = byteArray[1 + offset];
        mot135 = byteArray[2 + offset];
        mot225 = byteArray[3 + offset];
        mot315 = byteArray[4 + offset];
        motVert = byteArray[5 + offset];

        return true;
    }

    public int GetMot45()
    {
        return mot45;
    }

    public int GetMot135()
    {
        return mot135;
    }

    public int GetMot225()
    {
        return mot225;
    }

    public int GetMot315()
    {
        return mot315;
    }

    public int GetMotVert()
    {
        return motVert;
    }
}
