using System;
using System.Linq;
using UnityEngine;

namespace OnlineBoardGames.CantStop
{
    [Serializable]
    public class DiceData
    {
        int[] values;

        public int this[int index] => values[index];

        public DiceData(int[] values) => this.values = values;

        public bool IsValid => values[0] > 0;

        public int Sum => values.Sum();

        public static DiceData Empty()
        {
            int[] values = new int[4];

            for (int i = 0; i < values.Length; i++)
                values[i] = 0;

            return new(values);
        }

        public static DiceData Default()
        {
            DiceData data = Empty();
            int[] values = new int[4];

            for (int i = 0; i < values.Length; i++)
                values[i] = UnityEngine.Random.Range(1, 7);

            data.values = values;
            return data;
        }

        public override string ToString()
        {
            return string.Join('_', values);
        }
    }

    public static class DiceDataSerializer
    {
        public static DiceData ReadDiceData(this Mirror.NetworkReader reader)
        {
            Debug.Log($"Read DiceData");
            int[] values = new int[4];
            byte value1 = reader.ReadByte();
            byte value2 = reader.ReadByte();
            byte mask = 7;
            values[0] = value1 & mask;
            values[2] = value2 & mask;
            mask = 112;
            values[1] = ((value1 & mask) >> 4);
            values[3] = ((value2 & mask) >> 4);

            return new DiceData(values);
        }

        public static void WriteCardData(this Mirror.NetworkWriter writer, DiceData diceData)
        {
            Debug.Log($"Write DiceData");
            writer.WriteByte((byte)((diceData[0]) | (diceData[1] << 4)));
            writer.WriteByte((byte)((diceData[2]) | (diceData[3] << 4)));
        }
    }
}
