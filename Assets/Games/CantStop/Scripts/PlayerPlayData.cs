using FishNet.Serializing;
using System;
using UnityEngine;

namespace FunBoardGames.CantStop
{
    [Serializable]
    public class PlayerPlayData
    {
        public int DiceIndex1 { get; private set; }
        public int DiceIndex2 { get; private set; }
        public int ColumnIndex1 { get; private set; }
        public int? ColumnIndex2 { get; private set; }

        public PlayerPlayData(int diceIndex1, int diceIndex2, int columnIndex1, int? columnIndex2)
        {
            DiceIndex1 = diceIndex1;
            DiceIndex2 = diceIndex2;
            ColumnIndex1 = columnIndex1;
            ColumnIndex2 = columnIndex2;
        }
    }

    public static class PlayerDiceDataSerializer
    {
        public static PlayerPlayData ReadDiceData(this Reader reader)
        {
            Debug.Log($"Read PlayerDiceData");
            int diceValue1 = reader.ReadByte();

            if (diceValue1 == 255)
                return null;

            int diceValue2 = reader.ReadByte();
            int columnIndex1 = reader.ReadByte();
            int h = reader.ReadByte();
            int? columnIndex2 = (h == 255 ? null : h);
            return new PlayerPlayData(diceValue1, diceValue2, columnIndex1, columnIndex2);
        }

        public static void WriteCardData(this Writer writer, PlayerPlayData playerDiceData)
        {
            Debug.Log($"Write PlayerDiceData");

            if(playerDiceData == null)
            {
                writer.WriteByte(255);
                return;
            }

            writer.WriteByte((byte)playerDiceData.DiceIndex1);
            writer.WriteByte((byte)playerDiceData.DiceIndex2);
            writer.WriteByte((byte)playerDiceData.ColumnIndex1);
            writer.WriteByte(playerDiceData.ColumnIndex2 == null ? (byte)255 : (byte)(playerDiceData.ColumnIndex2.Value));
        }
    }
}
