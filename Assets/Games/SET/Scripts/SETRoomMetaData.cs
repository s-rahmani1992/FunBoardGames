using FishNet.Serializing;
using System;

namespace FunBoardGames.SET
{
    [Serializable]
    public class SETRoomMetaData
    {
        public float GuessTime { get; private set; }
        public int RoundCount { get; private set; }

        public SETRoomMetaData(float guessTime, int roundCount)
        {
            GuessTime = guessTime;
            RoundCount = roundCount;
        }

        public SETRoomMetaData(Reader reader)
        {
            GuessTime = reader.ReadSingle();
            RoundCount = reader.ReadByte();
        }
    }

    public static class SETRoomMetaDataSerializer
    {
        public static SETRoomMetaData ReadSETRoomMetaData(this Reader reader)
        {
            return new SETRoomMetaData(reader);
        }

        public static void WriteSETRoomMetaData(this Writer writer, SETRoomMetaData cardData)
        {
            writer.WriteSingle(cardData.GuessTime);
            writer.WriteByte((byte)cardData.RoundCount);
        }
    }
}
