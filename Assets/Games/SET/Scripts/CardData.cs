using System;

[System.Serializable]
public class CardData
{
    public byte color;
    public byte shape;
    public byte shading;
    public byte count;

    public CardData(byte byteIn){
        RefreshData(byteIn);
    }

    public void RefreshData(byte cardByte){
        byte mask = 3;
        color = (byte)(cardByte & mask);

        mask = 12;
        shape = (byte)((cardByte & mask) >> 2);

        mask = 48;
        shading = (byte)((cardByte & mask) >> 4);

        mask = 192;
        count = (byte)((cardByte & mask) >> 6);
    }

    public static bool isValid(byte cardByte){
        byte mask = 3;
        if ((cardByte & mask) == 3)
            return false;

        mask = 12;
        if ((cardByte & mask) == 12)
            return false;

        mask = 48;
        if ((cardByte & mask) == 48)
            return false;

        mask = 192;
        if ((cardByte & mask) == 0)
            return false;
        return true;
    }

    public override string ToString(){
        return $"({color}, {shape}, {shading}, {count})";
    }
}
