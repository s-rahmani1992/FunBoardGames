using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineBoardGames.SET
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

        public SETRoomMetaData(NetworkReader reader)
        {
            GuessTime = reader.ReadFloat();
            RoundCount = reader.ReadByte();
        }
    }

    public static class SETRoomMetaDataSerializer
    {
        public static SETRoomMetaData ReadSETRoomMetaData(this NetworkReader reader)
        {
            return new SETRoomMetaData(reader);
        }

        public static void WriteSETRoomMetaData(this NetworkWriter writer, SETRoomMetaData cardData)
        {
            writer.WriteFloat(cardData.GuessTime);
            writer.WriteByte((byte)cardData.RoundCount);
        }
    }
}
